using Microsoft.AspNetCore.Mvc;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ThAmCo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with optional filtering and search
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeOutOfStock = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var products = await _productService.GetProductsAsync(category, search, includeOutOfStock, page, pageSize);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Search products by name and description
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var products = await _productService.SearchProductsAsync(query, page, pageSize);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", query);
            return StatusCode(500, "An error occurred while searching products");
        }
    }

    /// <summary>
    /// Get product categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        try
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get stock status for a product (updated every 5 minutes)
    /// </summary>
    [HttpGet("{id}/stock")]
    public async Task<ActionResult<ProductStockDto>> GetProductStock(int id)
    {
        try
        {
            var stock = await _productService.GetProductStockAsync(id);
            if (stock == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(stock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock for product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving product stock");
        }
    }

    /// <summary>
    /// Restock a product (Staff only)
    /// </summary>
    [HttpPut("{id}/restock")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<ProductDto>> RestockProduct(int id, [FromBody] RestockProductDto restockDto)
    {
        try
        {
            var product = await _productService.RestockProductAsync(id, restockDto.Quantity);
            if (product == null)
            {
                return NotFound(new ErrorResponseDto { Message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product {ProductId}", id);
            return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while restocking the product" });
        }
    }
}
