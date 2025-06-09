namespace ThAmCo.WebApi.Services;

public interface ISupplierService
{
    Task<bool> PurchaseFromSupplierAsync(int productId, int quantity);
    Task UpdateSupplierStockAsync();
    Task UpdateSupplierPricesAsync();
} 