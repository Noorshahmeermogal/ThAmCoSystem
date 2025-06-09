using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalPrice { get; set; }
    
    public virtual Order Order { get; set; } = null!;
    
    public virtual Product Product { get; set; } = null!;
}
