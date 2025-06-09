using ThAmCo.WebApi.Services;

namespace ThAmCo.WebApi.Services;

public class StockUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

    public StockUpdateService(IServiceProvider serviceProvider, ILogger<StockUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Update Service started. Updates will run every {Interval} minutes", 
            _updateInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateStockLevels();
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stock Update Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Stock Update Service");
                // Continue running even if there's an error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task UpdateStockLevels()
    {
        using var scope = _serviceProvider.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
        var supplierService = scope.ServiceProvider.GetRequiredService<ISupplierService>();

        try
        {
            _logger.LogInformation("Starting scheduled stock update at {Time}", DateTime.UtcNow);

            // Update supplier stock levels first
            await supplierService.UpdateSupplierStockAsync();

            // Then update product stock levels based on supplier data
            await productService.UpdateProductStockAsync();

            _logger.LogInformation("Scheduled stock update completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete scheduled stock update");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Update Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
