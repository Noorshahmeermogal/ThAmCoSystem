using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class Supplier
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ApiEndpoint { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? ApiKey { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
