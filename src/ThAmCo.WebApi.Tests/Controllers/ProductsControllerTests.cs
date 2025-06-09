using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.DTOs;
using ThAmCo.WebApi.Models;
using Xunit;

namespace ThAmCo.WebApi.Tests.Controllers;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ThAmCoContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ThAmCoContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Ensure database is created and seeded
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ThAmCoContext>();
                SeedTestData(context);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchProducts_WithValidQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?query=wireless");
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(products);
        Assert.All(products, p => Assert.Contains("wireless", p.Name.ToLower()));
    }

    private static void SeedTestData(ThAmCoContext context)
    {
        if (context.Products.Any()) return;

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Wireless Headphones",
                Description = "High-quality wireless headphones",
                Category = "Electronics",
                BasePrice = 89.99m,
                CurrentPrice = 98.99m,
                StockQuantity = 50,
                IsActive = true
            },
            new()
            {
                Id = 2,
                Name = "Smartphone Case",
                Description = "Protective case for smartphones",
                Category = "Accessories",
                BasePrice = 19.99m,
                CurrentPrice = 21.99m,
                StockQuantity = 100,
                IsActive = true
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
