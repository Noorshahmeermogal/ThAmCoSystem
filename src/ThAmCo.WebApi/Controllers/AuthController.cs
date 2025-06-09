using Microsoft.AspNetCore.Mvc;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new customer
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterCustomerAsync(registerDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer registration");
            return StatusCode(500, new AuthResponseDto { Success = false, Message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Customer login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer login");
            return StatusCode(500, new AuthResponseDto { Success = false, Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Staff login
    /// </summary>
    [HttpPost("staff/login")]
    public async Task<ActionResult<AuthResponseDto>> StaffLogin([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.StaffLoginAsync(loginDto);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during staff login");
            return StatusCode(500, new AuthResponseDto { Success = false, Message = "An error occurred during staff login" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResponseDto { Success = false, Message = "An error occurred during token refresh" });
        }
    }
}
