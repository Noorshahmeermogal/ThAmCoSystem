using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? DispatchedDate { get; set; }
    
    public virtual Customer Customer { get; set; } = null!;
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
