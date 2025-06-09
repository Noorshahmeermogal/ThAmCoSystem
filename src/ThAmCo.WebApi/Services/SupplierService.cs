using Microsoft.EntityFrameworkCore;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.Models;

namespace ThAmCo.WebApi.Services;

public class SupplierService : ISupplierService
{
    private readonly ThAmCoContext _context;
    private readonly ILogger<SupplierService> _logger;
    private readonly HttpClient _httpClient;

    public SupplierService(ThAmCoContext context, ILogger<SupplierService> logger, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> PurchaseFromSupplierAsync(int productId, int quantity)
    {
        try
        {
            var productSuppliers = await _context.ProductSuppliers
                .Include(ps => ps.Supplier)
                .Where(ps => ps.ProductId == productId && ps.Supplier.IsActive)
                .OrderBy(ps => ps.SupplierPrice)
                .ToListAsync();

            foreach (var productSupplier in productSuppliers)
            {
                if (productSupplier.StockQuantity >= quantity)
                {
                    // Simulate API call to supplier
                    var purchaseRequest = new
                    {
                        ProductId = productSupplier.SupplierProductId,
                        Quantity = quantity,
                        RequestId = Guid.NewGuid().ToString()
                    };

                    try
                    {
                        // In a real implementation, this would be an actual HTTP call
                        _logger.LogInformation("Purchasing {Quantity} units of product {ProductId} from supplier {SupplierId}", 
                            quantity, productId, productSupplier.SupplierId);

                        // Simulate successful purchase
                        await Task.Delay(100); // Simulate network delay

                        // Update local stock
                        productSupplier.StockQuantity -= quantity;
                        productSupplier.LastUpdated = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Successfully purchased {Quantity} units from supplier {SupplierId}", 
                            quantity, productSupplier.SupplierId);
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to purchase from supplier {SupplierId}", productSupplier.SupplierId);
                        continue; // Try next supplier
                    }
                }
            }

            _logger.LogWarning("No supplier has sufficient stock for product {ProductId}, quantity {Quantity}", 
                productId, quantity);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during supplier purchase for product {ProductId}", productId);
            return false;
        }
    }

    public async Task UpdateSupplierStockAsync()
    {
        try
        {
            _logger.LogInformation("Starting supplier stock update");

            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .ToListAsync();

            foreach (var supplier in suppliers)
            {
                try
                {
                    // Simulate API call to get stock levels
                    _logger.LogInformation("Updating stock from supplier {SupplierId}: {SupplierName}", 
                        supplier.Id, supplier.Name);

                    var productSuppliers = await _context.ProductSuppliers
                        .Where(ps => ps.SupplierId == supplier.Id)
                        .ToListAsync();

                    foreach (var productSupplier in productSuppliers)
                    {
                        // Simulate random stock updates (in real implementation, this would come from supplier API)
                        var random = new Random();
                        var stockChange = random.Next(-5, 20); // Random change between -5 and +20
                        var newStock = Math.Max(0, productSupplier.StockQuantity + stockChange);

                        if (newStock != productSupplier.StockQuantity)
                        {
                            productSupplier.StockQuantity = newStock;
                            productSupplier.LastUpdated = DateTime.UtcNow;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Stock update completed for supplier {SupplierId}", supplier.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update stock for supplier {SupplierId}", supplier.Id);
                }
            }

            _logger.LogInformation("Supplier stock update completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during supplier stock update");
            throw;
        }
    }

    public async Task UpdateSupplierPricesAsync()
    {
        try
        {
            _logger.LogInformation("Starting supplier price update");

            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .ToListAsync();

            foreach (var supplier in suppliers)
            {
                try
                {
                    _logger.LogInformation("Updating prices from supplier {SupplierId}: {SupplierName}", 
                        supplier.Id, supplier.Name);

                    var productSuppliers = await _context.ProductSuppliers
                        .Where(ps => ps.SupplierId == supplier.Id)
                        .ToListAsync();

                    foreach (var productSupplier in productSuppliers)
                    {
                        // Simulate price fluctuations (in real implementation, this would come from supplier API)
                        var random = new Random();
                        var priceChangePercent = (random.NextDouble() - 0.5) * 0.1; // ±5% change
                        var newPrice = Math.Round(productSupplier.SupplierPrice * (1 + (decimal)priceChangePercent), 2);
                        
                        // Ensure minimum price
                        newPrice = Math.Max(0.01m, newPrice);

                        if (newPrice != productSupplier.SupplierPrice)
                        {
                            productSupplier.SupplierPrice = newPrice;
                            productSupplier.LastUpdated = DateTime.UtcNow;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Price update completed for supplier {SupplierId}", supplier.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update prices for supplier {SupplierId}", supplier.Id);
                }
            }

            _logger.LogInformation("Supplier price update completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during supplier price update");
            throw;
        }
    }
}
