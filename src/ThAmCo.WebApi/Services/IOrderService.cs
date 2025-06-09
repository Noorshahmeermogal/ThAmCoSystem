using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateOrderAsync(int customerId, CreateOrderDto createOrderDto);
    Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(int customerId, int page = 1, int pageSize = 20);
    Task<OrderDetailDto?> GetOrderByIdAsync(int id, int? customerId = null);
    Task<IEnumerable<OrderDto>> GetPendingDispatchOrdersAsync(int page = 1, int pageSize = 20);
    Task<OrderDto?> DispatchOrderAsync(int id, string dispatchedBy);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string? status = null, int page = 1, int pageSize = 20);
} 