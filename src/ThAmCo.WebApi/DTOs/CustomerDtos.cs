using System.ComponentModel.DataAnnotations;
using ThAmCo.WebApi.Models;

namespace ThAmCo.WebApi.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PaymentAddress { get; set; }
}

public class CustomerDetailDto : CustomerDto
{
    public decimal AccountFunds { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
}

public class CustomerFundsDto
{
    public int CustomerId { get; set; }
    public decimal AccountFunds { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class UpdateCustomerDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public string? DeliveryAddress { get; set; }
    public string? PaymentAddress { get; set; }
}

public class AddFundsDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}

public class CustomerAddFundsDto
{
    [Required]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}

public class CustomerSearchDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
