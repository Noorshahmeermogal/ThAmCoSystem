using System.ComponentModel.DataAnnotations;

namespace ThAmCo.WebApi.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DispatchedDate { get; set; }
    public int ItemCount { get; set; }
    public string? CustomerName { get; set; }
    public int CustomerId { get; set; }
}

public class OrderDetailDto : OrderDto
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderDto
{
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();

    [Required]
    [MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CreateOrderItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class OrderSearchDto
{
    public string? OrderNumber { get; set; }
    public string? Status { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DispatchOrderDto
{
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
}
