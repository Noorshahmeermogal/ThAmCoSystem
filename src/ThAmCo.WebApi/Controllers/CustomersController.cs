using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get current customer profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<CustomerDto>> GetProfile()
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer profile");
            return StatusCode(500, "An error occurred while retrieving profile");
        }
    }

    /// <summary>
    /// Update customer profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<CustomerDto>> UpdateProfile([FromBody] UpdateCustomerDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = GetCurrentCustomerId();
            var updatedCustomer = await _customerService.UpdateCustomerAsync(customerId, updateDto);
            
            if (updatedCustomer == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(updatedCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile");
            return StatusCode(500, "An error occurred while updating profile");
        }
    }

    /// <summary>
    /// Get customer account funds
    /// </summary>
    [HttpGet("funds")]
    public async Task<ActionResult<CustomerFundsDto>> GetFunds()
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var funds = await _customerService.GetCustomerFundsAsync(customerId);
            
            if (funds == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(funds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer funds");
            return StatusCode(500, "An error occurred while retrieving funds");
        }
    }

    /// <summary>
    /// Request account deletion
    /// </summary>
    [HttpDelete("account")]
    public async Task<ActionResult> DeleteAccount()
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var result = await _customerService.RequestAccountDeletionAsync(customerId);
            
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok(new { message = "Account deletion requested successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting account deletion");
            return StatusCode(500, "An error occurred while processing account deletion request");
        }
    }

    /// <summary>
    /// Get all customers (Staff only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync(page, pageSize);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            return StatusCode(500, "An error occurred while retrieving customers");
        }
    }

    /// <summary>
    /// Get specific customer details (Staff only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<CustomerDetailDto>> GetCustomer(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerDetailAsync(id);
            
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return StatusCode(500, "An error occurred while retrieving customer");
        }
    }

    /// <summary>
    /// Add funds to customer account (Staff only)
    /// </summary>
    [HttpPost("add-funds")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult<CustomerDto>> AddFunds([FromBody] AddFundsDto addFundsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var staffEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var updatedCustomer = await _customerService.AddFundsAsync(addFundsDto.CustomerId, addFundsDto.Amount, staffEmail);
            
            if (updatedCustomer == null)
            {
                return NotFound($"Customer with ID {addFundsDto.CustomerId} not found");
            }

            return Ok(updatedCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding funds to customer {CustomerId}", addFundsDto.CustomerId);
            return StatusCode(500, "An error occurred while adding funds");
        }
    }

    /// <summary>
    /// Add funds to customer account (Customer self-service)
    /// </summary>
    [HttpPost("self-add-funds")]
    public async Task<ActionResult<CustomerDto>> CustomerAddFunds([FromBody] CustomerAddFundsDto addFundsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var customerId = GetCurrentCustomerId();
            var updatedCustomer = await _customerService.CustomerAddFundsAsync(customerId, addFundsDto.Amount);
            
            if (updatedCustomer == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "Unable to add funds. Please ensure the amount is positive." });
            }

            return Ok(updatedCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding funds by customer {CustomerId}", GetCurrentCustomerId());
            return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while adding funds." });
        }
    }

    /// <summary>
    /// Delete customer account (Staff only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff,Administrator")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        try
        {
            var staffEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var result = await _customerService.DeleteCustomerAsync(id, staffEmail);
            
            if (!result)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(new { message = "Customer account deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, "An error occurred while deleting customer");
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
