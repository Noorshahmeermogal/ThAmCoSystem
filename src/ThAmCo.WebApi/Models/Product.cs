using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal CurrentPrice { get; set; }
    
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    public DateTime LastStockUpdate { get; set; } = DateTime.UtcNow;
    
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
}
