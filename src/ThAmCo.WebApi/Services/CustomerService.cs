using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.DTOs;
using ThAmCo.WebApi.Models;

namespace ThAmCo.WebApi.Services;

public class CustomerService : ICustomerService
{
    private readonly ThAmCoContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ThAmCoContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == id && c.IsActive)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                DeliveryAddress = c.DeliveryAddress,
                PaymentAddress = c.PaymentAddress
            })
            .FirstOrDefaultAsync();

        return customer;
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (customer == null) return null;

        // Store old values for audit
        var oldValues = new
        {
            customer.Name,
            customer.PhoneNumber,
            customer.DeliveryAddress
        };

        // Update customer
        customer.Name = updateDto.Name;
        customer.PhoneNumber = updateDto.PhoneNumber;
        customer.DeliveryAddress = updateDto.DeliveryAddress;
        customer.UpdatedAt = DateTime.UtcNow;

        // Create audit log
        var auditLog = new CustomerAuditLog
        {
            CustomerId = customer.Id,
            Action = "Profile Updated",
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(new { updateDto.Name, updateDto.PhoneNumber, updateDto.DeliveryAddress }),
            ChangedBy = customer.Email,
            ChangedAt = DateTime.UtcNow
        };

        _context.CustomerAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DeliveryAddress = customer.DeliveryAddress,
            PaymentAddress = customer.PaymentAddress
        };
    }

    public async Task<CustomerFundsDto?> GetCustomerFundsAsync(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == id && c.IsActive)
            .Select(c => new CustomerFundsDto
            {
                CustomerId = c.Id,
                AccountFunds = c.AccountFunds,
                LastUpdated = c.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return customer;
    }

    public async Task<bool> RequestAccountDeletionAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (customer == null) return false;

        // Create audit log for deletion request
        var auditLog = new CustomerAuditLog
        {
            CustomerId = customer.Id,
            Action = "Account Deletion Requested",
            OldValues = JsonSerializer.Serialize(new { customer.Name, customer.Email }),
            ChangedBy = customer.Email,
            ChangedAt = DateTime.UtcNow
        };

        _context.CustomerAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Account deletion requested for customer {CustomerId}", id);
        return true;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int page = 1, int pageSize = 20)
    {
        var customers = await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                DeliveryAddress = c.DeliveryAddress,
                PaymentAddress = c.PaymentAddress
            })
            .ToListAsync();

        return customers;
    }

    public async Task<CustomerDetailDto?> GetCustomerDetailAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .Where(c => c.Id == id && c.IsActive)
            .FirstOrDefaultAsync();

        if (customer == null) return null;

        return new CustomerDetailDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DeliveryAddress = customer.DeliveryAddress,
            PaymentAddress = customer.PaymentAddress,
            AccountFunds = customer.AccountFunds,
            CreatedAt = customer.CreatedAt,
            TotalOrders = customer.Orders.Count,
            TotalSpent = customer.Orders.Sum(o => o.TotalAmount),
            RecentOrders = customer.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate
                })
                .ToList()
        };
    }

    public async Task<bool> DeleteCustomerAsync(int id, string deletedBy)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (customer == null) return false;

        // Store original data for audit
        var originalData = new
        {
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            customer.DeliveryAddress,
            customer.PaymentAddress
        };

        // Anonymize personal data but retain account for order history
        customer.Name = $"Deleted User {customer.Id}";
        customer.Email = $"deleted.{customer.Id}@anonymized.com";
        customer.PhoneNumber = null;
        customer.DeliveryAddress = "Address Deleted";
        customer.PaymentAddress = "Address Deleted";
        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        // Create audit log
        var auditLog = new CustomerAuditLog
        {
            CustomerId = customer.Id,
            Action = "Account Deleted",
            OldValues = JsonSerializer.Serialize(originalData),
            NewValues = "Data Anonymized",
            ChangedBy = deletedBy,
            ChangedAt = DateTime.UtcNow
        };

        _context.CustomerAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} deleted by {DeletedBy}", id, deletedBy);
        return true;
    }

    public async Task<CustomerDto?> AddFundsAsync(int customerId, decimal amount, string staffEmail)
    {
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null) return null;

        // Store old funds for audit
        var oldFunds = customer.AccountFunds;

        customer.AccountFunds += amount;
        customer.UpdatedAt = DateTime.UtcNow;

        // Create audit log
        var auditLog = new CustomerAuditLog
        {
            CustomerId = customer.Id,
            Action = "Funds Added",
            OldValues = oldFunds.ToString("F2"),
            NewValues = customer.AccountFunds.ToString("F2"),
            ChangedBy = staffEmail,
            ChangedAt = DateTime.UtcNow
        };

        _context.CustomerAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DeliveryAddress = customer.DeliveryAddress,
            PaymentAddress = customer.PaymentAddress
        };
    }

    public async Task<CustomerDto?> CustomerAddFundsAsync(int customerId, decimal amount)
    {
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null) return null;

        if (amount <= 0) 
        {
            _logger.LogWarning("Invalid attempt to add non-positive funds for customer {CustomerId}", customerId);
            return null; // Or throw an exception, depending on desired error handling
        }

        // Store old funds for audit
        var oldFunds = customer.AccountFunds;

        customer.AccountFunds += amount;
        customer.UpdatedAt = DateTime.UtcNow;

        // Create audit log
        var auditLog = new CustomerAuditLog
        {
            CustomerId = customer.Id,
            Action = "Funds Added by Customer",
            OldValues = oldFunds.ToString("F2"),
            NewValues = customer.AccountFunds.ToString("F2"),
            ChangedBy = customer.Email, // Customer's own email as changed by
            ChangedAt = DateTime.UtcNow
        };

        _context.CustomerAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DeliveryAddress = customer.DeliveryAddress,
            PaymentAddress = customer.PaymentAddress
        };
    }
}
