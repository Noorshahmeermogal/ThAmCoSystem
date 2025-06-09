using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsInStock { get; set; }
    public DateTime LastStockUpdate { get; set; }
}

public class ProductStockDto
{
    public int ProductId { get; set; }
    public int StockQuantity { get; set; }
    public bool IsInStock { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CreateProductDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }
    
    [Range(0, int.MaxValue)]
    public int InitialStock { get; set; } = 0;
}

public class UpdateProductDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }
}

public class ProductSearchDto
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool IncludeOutOfStock { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RestockProductDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
