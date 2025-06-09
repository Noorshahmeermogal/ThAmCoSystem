using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class ProductSupplier
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int SupplierId { get; set; }
    
    [MaxLength(100)]
    public string? SupplierProductId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal SupplierPrice { get; set; }
    
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public virtual Product Product { get; set; } = null!;
    
    public virtual Supplier Supplier { get; set; } = null!;
}
