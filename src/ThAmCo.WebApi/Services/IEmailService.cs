using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string email, OrderDto order);
    Task SendOrderStatusUpdateAsync(string email, OrderDto order);
} 