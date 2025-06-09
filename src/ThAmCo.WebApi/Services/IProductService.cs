using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(string? category = null, string? search = null, bool includeOutOfStock = true, int page = 1, int pageSize = 20);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string query, int page = 1, int pageSize = 20);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<ProductStockDto?> GetProductStockAsync(int id);
    Task UpdateProductStockAsync();
    Task UpdateProductCatalogAsync();
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductSearchDto? searchDto = null);
    Task<IEnumerable<string>> GetProductCategoriesAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int id);
    Task<ProductDto?> RestockProductAsync(int id, int quantity);
}
