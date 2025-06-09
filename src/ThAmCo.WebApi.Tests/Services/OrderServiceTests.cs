using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.Models;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.DTOs;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ThAmCo.WebApi.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<ISupplierService> _supplierServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ThAmCoContext _context;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ThAmCoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ThAmCoContext(options);
        _loggerMock = new Mock<ILogger<OrderService>>();
        _supplierServiceMock = new Mock<ISupplierService>();
        _emailServiceMock = new Mock<IEmailService>();
        _orderService = new OrderService(_context, _supplierServiceMock.Object, _emailServiceMock.Object, _loggerMock.Object);

        SeedTestData();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ReturnsOrder()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            DeliveryAddress = "123 Test St",
            PhoneNumber = "1234567890",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 1 }
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(1, createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CustomerId);
        Assert.Single(result.OrderItems);
        Assert.Equal(120.00m, result.TotalAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_InsufficientStock_ReturnsNull()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            DeliveryAddress = "123 Test St",
            PhoneNumber = "1234567890",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 20 } // Only 10 in stock
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(1, createOrderDto));
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_ReturnsOrdersForCustomer()
    {
        // Act
        var result = await _orderService.GetCustomerOrdersAsync(1);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, o => Assert.Equal(1, o.CustomerId));
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithValidIdAndCustomerId_ReturnsOrder()
    {
        // Act
        var result = await _orderService.GetOrderByIdAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.CustomerId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _orderService.GetOrderByIdAsync(999, 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithMismatchedCustomerId_ReturnsNull()
    {
        // Act
        var result = await _orderService.GetOrderByIdAsync(1, 2); // Order 1 belongs to customer 1

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPendingDispatchOrdersAsync_ReturnsPendingOrders()
    {
        // Act
        var result = await _orderService.GetPendingDispatchOrdersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, o => Assert.Equal("Pending", o.Status));
    }

    [Fact]
    public async Task DispatchOrderAsync_WithValidId_UpdatesOrderStatus()
    {
        // Arrange
        var order = _context.Orders.First();
        Assert.Equal("Pending", order.Status);

        // Act
        var result = await _orderService.DispatchOrderAsync(order.Id, "staff@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Dispatched", result.Status);
        Assert.NotNull(result.DispatchedDate);
    }

    [Fact]
    public async Task DispatchOrderAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _orderService.DispatchOrderAsync(999, "staff@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count());
    }

    [Fact]
    public async Task GetAllOrdersAsync_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var dispatchedOrder = new Order
        {
            Id = 2,
            CustomerId = 1,
            OrderNumber = "ORD002",
            TotalAmount = 50.00m,
            Status = "Dispatched",
            DeliveryAddress = "456 Other Rd",
            PhoneNumber = "0987654321",
            OrderDate = DateTime.UtcNow.AddDays(-2),
            DispatchedDate = DateTime.UtcNow.AddDays(-1)
        };
        _context.Orders.Add(dispatchedOrder);
        _context.SaveChanges();

        // Act
        var result = await _orderService.GetAllOrdersAsync("Dispatched");

        // Assert
        Assert.Single(result);
        Assert.Equal("Dispatched", result.First().Status);
    }

    private void SeedTestData()
    {
        _context.Customers.Add(new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            AccountFunds = 500.00m,
            PaymentAddress = "123 Main St",
            IsActive = true
        });
        _context.Products.Add(new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "A test product",
            Category = "Test",
            BasePrice = 100.00m,
            CurrentPrice = 120.00m,
            StockQuantity = 10,
            IsActive = true
        });
        _context.Orders.Add(new Order
        {
            Id = 1,
            CustomerId = 1,
            OrderNumber = "ORD001",
            TotalAmount = 120.00m,
            Status = "Pending",
            DeliveryAddress = "123 Main St",
            PhoneNumber = "1234567890",
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 120.00m }
            }
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}