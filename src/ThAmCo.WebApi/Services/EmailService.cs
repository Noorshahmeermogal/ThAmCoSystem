using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendOrderConfirmationAsync(string email, OrderDto order)
    {
        try
        {
            // In a real implementation, you would use an email service like SendGrid, AWS SES, etc.
            _logger.LogInformation("Sending order confirmation email to {Email} for order {OrderNumber}", 
                email, order.OrderNumber);

            // Simulate email sending
            await Task.Delay(100);

            var emailContent = $@"
                Dear Customer,
                
                Thank you for your order!
                
                Order Details:
                Order Number: {order.OrderNumber}
                Total Amount: £{order.TotalAmount:F2}
                Order Date: {order.OrderDate:yyyy-MM-dd HH:mm}
                Status: {order.Status}
                
                We will notify you when your order is dispatched.
                
                Best regards,
                ThAmCo Team
            ";

            _logger.LogInformation("Order confirmation email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email to {Email}", email);
        }
    }

    public async Task SendOrderStatusUpdateAsync(string email, OrderDto order)
    {
        try
        {
            _logger.LogInformation("Sending order status update email to {Email} for order {OrderNumber}", 
                email, order.OrderNumber);

            // Simulate email sending
            await Task.Delay(100);

            var emailContent = $@"
                Dear Customer,
                
                Your order status has been updated!
                
                Order Details:
                Order Number: {order.OrderNumber}
                New Status: {order.Status}
                {(order.DispatchedDate.HasValue ? $"Dispatched Date: {order.DispatchedDate:yyyy-MM-dd HH:mm}" : "")}
                
                Thank you for choosing ThAmCo!
                
                Best regards,
                ThAmCo Team
            ";

            _logger.LogInformation("Order status update email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order status update email to {Email}", email);
        }
    }
}
