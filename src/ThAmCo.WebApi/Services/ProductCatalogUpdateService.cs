using ThAmCo.WebApi.Services;

namespace ThAmCo.WebApi.Services;

public class ProductCatalogUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductCatalogUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(24); // Daily updates

    public ProductCatalogUpdateService(IServiceProvider serviceProvider, ILogger<ProductCatalogUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Product Catalog Update Service started. Updates will run every {Interval} hours", 
            _updateInterval.TotalHours);

        // Wait for initial delay to avoid startup conflicts
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateProductCatalog();
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Product Catalog Update Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Product Catalog Update Service");
                // Continue running even if there's an error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task UpdateProductCatalog()
    {
        using var scope = _serviceProvider.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
        var supplierService = scope.ServiceProvider.GetRequiredService<ISupplierService>();

        try
        {
            _logger.LogInformation("Starting scheduled product catalog update at {Time}", DateTime.UtcNow);

            // Update supplier prices first
            await supplierService.UpdateSupplierPricesAsync();

            // Then update product catalog and prices
            await productService.UpdateProductCatalogAsync();

            _logger.LogInformation("Scheduled product catalog update completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete scheduled product catalog update");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Product Catalog Update Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
