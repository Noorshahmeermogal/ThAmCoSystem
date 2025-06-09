using Microsoft.EntityFrameworkCore;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.DTOs;
using ThAmCo.WebApi.Models;

namespace ThAmCo.WebApi.Services;

public class OrderService : IOrderService
{
    private readonly ThAmCoContext _context;
    private readonly ISupplierService _supplierService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ThAmCoContext context,
        ISupplierService supplierService,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _supplierService = supplierService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<OrderDto?> CreateOrderAsync(int customerId, CreateOrderDto createOrderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get customer with validation
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId && c.IsActive);

            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found");
            }

            // Validate customer profile completeness
            if (string.IsNullOrWhiteSpace(customer.DeliveryAddress) || 
                string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                throw new InvalidOperationException("Delivery address and phone number must be set in profile");
            }

            // Calculate total and validate items
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in createOrderDto.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive);

                if (product == null)
                {
                    throw new InvalidOperationException($"Product {item.ProductId} not found");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                }

                var itemTotal = product.CurrentPrice * item.Quantity;
                totalAmount += itemTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.CurrentPrice,
                    TotalPrice = itemTotal
                });
            }

            // Check customer funds
            if (customer.AccountFunds < totalAmount)
            {
                throw new InvalidOperationException("Insufficient funds");
            }

            // Purchase from suppliers
            foreach (var item in createOrderDto.Items)
            {
                var success = await _supplierService.PurchaseFromSupplierAsync(item.ProductId, item.Quantity);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to purchase product {item.ProductId} from supplier");
                }
            }

            // Create order
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = GenerateOrderNumber(),
                TotalAmount = totalAmount,
                Status = "Pending",
                DeliveryAddress = customer.DeliveryAddress,
                PhoneNumber = customer.PhoneNumber,
                OrderDate = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);

            // Update customer funds
            customer.AccountFunds -= totalAmount;
            customer.UpdatedAt = DateTime.UtcNow;

            // Update product stock
            foreach (var item in createOrderDto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send confirmation email
            var orderDto = await GetOrderByIdAsync(order.Id);
            if (orderDto != null)
            {
                await _emailService.SendOrderConfirmationAsync(customer.Email, MapToOrderDto(order));
            }

            return MapToOrderDto(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(int customerId, int page = 1, int pageSize = 20)
    {
        var orders = await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderDate = o.OrderDate,
                DispatchedDate = o.DispatchedDate,
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync();

        return orders;
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(int id, int? customerId = null)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            DeliveryAddress = order.DeliveryAddress,
            PhoneNumber = order.PhoneNumber,
            OrderDate = order.OrderDate,
            DispatchedDate = order.DispatchedDate,
            Customer = new CustomerDto
            {
                Id = order.Customer.Id,
                Name = order.Customer.Name,
                Email = order.Customer.Email
            },
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderDto>> GetPendingDispatchOrdersAsync(int page = 1, int pageSize = 20)
    {
        var orders = await _context.Orders
            .Where(o => o.Status == "Pending")
            .OrderBy(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderDate = o.OrderDate,
                ItemCount = o.OrderItems.Count,
                CustomerName = o.Customer.Name
            })
            .ToListAsync();

        return orders;
    }

    public async Task<OrderDto?> DispatchOrderAsync(int id, string dispatchedBy)
    {
        var order = await _context.Orders.FindAsync(id);
        
        if (order == null || order.Status != "Pending")
        {
            return null;
        }

        order.Status = "Dispatched";
        order.DispatchedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send status update email
        var customer = await _context.Customers.FindAsync(order.CustomerId);
        if (customer != null)
        {
            await _emailService.SendOrderStatusUpdateAsync(customer.Email, MapToOrderDto(order));
        }

        return MapToOrderDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderDate = o.OrderDate,
                DispatchedDate = o.DispatchedDate,
                ItemCount = o.OrderItems.Count,
                CustomerName = o.Customer.Name
            })
            .ToListAsync();

        return orders;
    }

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            OrderDate = order.OrderDate,
            DispatchedDate = order.DispatchedDate,
            ItemCount = order.OrderItems?.Count ?? 0
        };
    }
}
