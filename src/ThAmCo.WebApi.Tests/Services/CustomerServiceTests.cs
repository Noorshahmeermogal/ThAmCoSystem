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

namespace ThAmCo.WebApi.Tests.Services;

public class CustomerServiceTests : IDisposable
{
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly ThAmCoContext _context;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ThAmCoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ThAmCoContext(options);
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _customerService = new CustomerService(_context, _loggerMock.Object);

        SeedTestData();
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithValidId_ReturnsCustomer()
    {
        // Act
        var result = await _customerService.GetCustomerByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Customer One", result.Name);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _customerService.GetCustomerByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithValidData_ReturnsUpdatedCustomer()
    {
        // Arrange
        var updateDto = new UpdateCustomerDto
        {
            Name = "Updated Customer One",
            PaymentAddress = "Updated Address"
        };

        // Act
        var result = await _customerService.UpdateCustomerAsync(1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Customer One", result.Name);
        Assert.Equal("Updated Address", result.PaymentAddress);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateCustomerDto
        {
            Name = "Updated Customer One",
            PaymentAddress = "Updated Address"
        };

        // Act
        var result = await _customerService.UpdateCustomerAsync(999, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerFundsAsync_WithValidId_ReturnsFunds()
    {
        // Act
        var result = await _customerService.GetCustomerFundsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100.00m, result.AccountFunds);
    }

    [Fact]
    public async Task GetCustomerFundsAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _customerService.GetCustomerFundsAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RequestAccountDeletionAsync_WithValidId_ReturnsTrue()
    {
        // Act
        var result = await _customerService.RequestAccountDeletionAsync(1);

        // Assert
        Assert.True(result);
        var customer = await _context.Customers.FindAsync(1);
        Assert.False(customer?.IsActive);
    }

    [Fact]
    public async Task RequestAccountDeletionAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _customerService.RequestAccountDeletionAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers()
    {
        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithValidId_ReturnsCustomerDetail()
    {
        // Act
        var result = await _customerService.GetCustomerDetailAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Customer One", result.Name);
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _customerService.GetCustomerDetailAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithValidId_ReturnsTrue()
    {
        // Act
        var result = await _customerService.DeleteCustomerAsync(1, "staff@example.com");

        // Assert
        Assert.True(result);
        var customer = await _context.Customers.FindAsync(1);
        Assert.False(customer?.IsActive);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _customerService.DeleteCustomerAsync(999, "staff@example.com");

        // Assert
        Assert.False(result);
    }

    private void SeedTestData()
    {
        _context.Customers.AddRange(
            new Customer
            {
                Id = 1,
                Name = "Customer One",
                Email = "customer1@example.com",
                AccountFunds = 100.00m,
                PaymentAddress = "Address 1",
                IsActive = true
            },
            new Customer
            {
                Id = 2,
                Name = "Customer Two",
                Email = "customer2@example.com",
                AccountFunds = 200.00m,
                PaymentAddress = "Address 2",
                IsActive = true
            }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 