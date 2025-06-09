using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCurrentCustomerId();
            var order = await _orderService.CreateOrderAsync(customerId, createOrderDto);
            
            if (order == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "Unable to create order. Please check product availability and account funds." });
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid order creation attempt");
            return BadRequest(new ErrorResponseDto { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Get customer's order history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrderHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var orders = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order history");
            return StatusCode(500, "An error occurred while retrieving order history");
        }
    }

    /// <summary>
    /// Get specific order details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var order = await _orderService.GetOrderByIdAsync(id, customerId);
            
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }

    /// <summary>
    /// Get orders pending dispatch (Staff only)
    /// </summary>
    [HttpGet("pending-dispatch")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetPendingDispatchOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetPendingDispatchOrdersAsync(page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending dispatch orders");
            return StatusCode(500, "An error occurred while retrieving pending orders");
        }
    }

    /// <summary>
    /// Mark order as dispatched (Staff only)
    /// </summary>
    [HttpPut("{id}/dispatch")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<OrderDto>> DispatchOrder(int id)
    {
        try
        {
            var staffEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var order = await _orderService.DispatchOrderAsync(id, staffEmail);
            
            if (order == null)
            {
                return NotFound(new ErrorResponseDto { Message = $"Order with ID {id} not found" });
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching order {OrderId}", id);
            return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while dispatching the order" });
        }
    }

    /// <summary>
    /// Get all orders (Staff only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync(status, page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while retrieving orders" });
        }
    }

    private int GetCurrentCustomerId()
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (int.TryParse(customerIdClaim, out int customerId))
        {
            return customerId;
        }
        throw new UnauthorizedAccessException("Invalid customer ID in token");
    }
}
