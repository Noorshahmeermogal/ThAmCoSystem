using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.Models;

public class CustomerAuditLog
{
    public int Id { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [MaxLength(255)]
    public string? ChangedBy { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
