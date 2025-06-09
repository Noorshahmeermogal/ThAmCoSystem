using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThAmCo.WebApi.Models;

public class Customer
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public string? DeliveryAddress { get; set; }
    
    public string? PaymentAddress { get; set; }
    
    [JsonIgnore]
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal AccountFunds { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
