using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.Models;
using ThAmCo.WebApi.Services;
using Xunit;

namespace ThAmCo.WebApi.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ThAmCoContext _context;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ThAmCoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ThAmCoContext(options);
        _loggerMock = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_context, _loggerMock.Object);

        SeedTestData();
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsAllActiveProducts()
    {
        // Act
        var result = await _productService.GetProductsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, p => Assert.True(p.Id > 0));
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product 1", result.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _productService.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchProductsAsync_WithValidQuery_ReturnsMatchingProducts()
    {
        // Act
        var result = await _productService.SearchProductsAsync("Test");

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, p => Assert.Contains("Test", p.Name));
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsDistinctCategories()
    {
        // Act
        var result = await _productService.GetCategoriesAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Electronics", result);
        Assert.Contains("Accessories", result);
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Test Product 1",
                Description = "Test Description 1",
                Category = "Electronics",
                BasePrice = 100.00m,
                CurrentPrice = 110.00m,
                StockQuantity = 10,
                IsActive = true
            },
            new()
            {
                Id = 2,
                Name = "Test Product 2",
                Description = "Test Description 2",
                Category = "Accessories",
                BasePrice = 50.00m,
                CurrentPrice = 55.00m,
                StockQuantity = 20,
                IsActive = true
            },
            new()
            {
                Id = 3,
                Name = "Inactive Product",
                Description = "This product is inactive",
                Category = "Electronics",
                BasePrice = 75.00m,
                CurrentPrice = 82.50m,
                StockQuantity = 5,
                IsActive = false
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
