using Microsoft.EntityFrameworkCore;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.DTOs;
using ThAmCo.WebApi.Models;

namespace ThAmCo.WebApi.Services;

public class ProductService : IProductService
{
    private readonly ThAmCoContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ThAmCoContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(string? category = null, string? search = null, bool includeOutOfStock = true, int page = 1, int pageSize = 20)
    {
        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
        }

        if (!includeOutOfStock)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.CurrentPrice,
                StockQuantity = p.StockQuantity,
                IsInStock = p.StockQuantity > 0,
                LastStockUpdate = p.LastStockUpdate
            })
            .ToListAsync();

        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.CurrentPrice,
                StockQuantity = p.StockQuantity,
                IsInStock = p.StockQuantity > 0,
                LastStockUpdate = p.LastStockUpdate
            })
            .FirstOrDefaultAsync();

        return product;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query, int page = 1, int pageSize = 20)
    {
        var products = await _context.Products
            .Where(p => p.IsActive && 
                       (p.Name.Contains(query) || 
                        (p.Description != null && p.Description.Contains(query))))
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.CurrentPrice,
                StockQuantity = p.StockQuantity,
                IsInStock = p.StockQuantity > 0,
                LastStockUpdate = p.LastStockUpdate
            })
            .ToListAsync();

        return products;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var categories = await _context.Products
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return categories;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductSearchDto? searchDto = null)
    {
        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (searchDto != null)
        {
            if (!string.IsNullOrWhiteSpace(searchDto.Query))
            {
                query = query.Where(p => p.Name.Contains(searchDto.Query) || (p.Description != null && p.Description.Contains(searchDto.Query)));
            }
            if (!string.IsNullOrWhiteSpace(searchDto.Category))
            {
                query = query.Where(p => p.Category == searchDto.Category);
            }
            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.CurrentPrice >= searchDto.MinPrice.Value);
            }
            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.CurrentPrice <= searchDto.MaxPrice.Value);
            }
            if (!searchDto.IncludeOutOfStock)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((searchDto?.Page - 1 ?? 0) * (searchDto?.PageSize ?? 20))
            .Take(searchDto?.PageSize ?? 20)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.CurrentPrice,
                StockQuantity = p.StockQuantity,
                IsInStock = p.StockQuantity > 0,
                LastStockUpdate = p.LastStockUpdate
            })
            .ToListAsync();

        return products;
    }

    public async Task<IEnumerable<string>> GetProductCategoriesAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<ProductStockDto?> GetProductStockAsync(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductStockDto
            {
                ProductId = p.Id,
                StockQuantity = p.StockQuantity,
                IsInStock = p.StockQuantity > 0,
                LastUpdated = p.LastStockUpdate
            })
            .FirstOrDefaultAsync();

        return product;
    }

    public async Task UpdateProductStockAsync()
    {
        try
        {
            _logger.LogInformation("Starting stock update process");

            var products = await _context.Products
                .Include(p => p.ProductSuppliers)
                .ThenInclude(ps => ps.Supplier)
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var product in products)
            {
                var totalStock = product.ProductSuppliers
                    .Where(ps => ps.Supplier.IsActive)
                    .Sum(ps => ps.StockQuantity);

                if (product.StockQuantity != totalStock)
                {
                    product.StockQuantity = totalStock;
                    product.LastStockUpdate = DateTime.UtcNow;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Stock update completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock update");
            throw;
        }
    }

    public async Task UpdateProductCatalogAsync()
    {
        try
        {
            _logger.LogInformation("Starting product catalog update");

            var products = await _context.Products
                .Include(p => p.ProductSuppliers)
                .ThenInclude(ps => ps.Supplier)
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var product in products)
            {
                var activeSuppliers = product.ProductSuppliers
                    .Where(ps => ps.Supplier.IsActive)
                    .ToList();

                if (activeSuppliers.Any())
                {
                    // Find cheapest supplier price and add 10%
                    var cheapestPrice = activeSuppliers.Min(ps => ps.SupplierPrice);
                    var newPrice = Math.Round(cheapestPrice * 1.10m, 2);

                    if (product.CurrentPrice != newPrice)
                    {
                        product.CurrentPrice = newPrice;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Product catalog update completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during product catalog update");
            throw;
        }
    }

    public async Task<ProductDto?> RestockProductAsync(int id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return null;
        }

        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        product.LastStockUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.CurrentPrice,
            StockQuantity = product.StockQuantity,
            IsInStock = product.StockQuantity > 0,
            LastStockUpdate = product.LastStockUpdate
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Category = createProductDto.Category,
            BasePrice = createProductDto.BasePrice,
            CurrentPrice = Math.Round(createProductDto.BasePrice * 1.10m, 2), // 10% markup
            StockQuantity = createProductDto.InitialStock,
            LastStockUpdate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.CurrentPrice,
            StockQuantity = product.StockQuantity,
            IsInStock = product.StockQuantity > 0,
            LastStockUpdate = product.LastStockUpdate
        };
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return null;

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Category = updateProductDto.Category;
        product.BasePrice = updateProductDto.BasePrice;
        product.CurrentPrice = Math.Round(updateProductDto.BasePrice * 1.10m, 2);
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.CurrentPrice,
            StockQuantity = product.StockQuantity,
            IsInStock = product.StockQuantity > 0,
            LastStockUpdate = product.LastStockUpdate
        };
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return false;

        product.IsActive = false; // Soft delete
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
