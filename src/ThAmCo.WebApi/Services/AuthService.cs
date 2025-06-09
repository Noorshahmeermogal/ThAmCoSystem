using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.DTOs;
using ThAmCo.WebApi.Models;
using BCrypt.Net;

namespace ThAmCo.WebApi.Services;

public class AuthService : IAuthService
{
    private readonly ThAmCoContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ThAmCoContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if customer already exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == registerDto.Email);

            if (existingCustomer != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Customer with this email already exists"
                };
            }

            // Create new customer
            var customer = new Customer
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PaymentAddress = registerDto.PaymentAddress,
                AccountFunds = 0, // Initial funds
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(customer.Id, customer.Email, "Customer");
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Role = "Customer"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer registration");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Registration failed"
            };
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == loginDto.Email && c.IsActive);

            if (customer == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var token = GenerateJwtToken(customer.Id, customer.Email, "Customer");
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Role = "Customer"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer login");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Login failed"
            };
        }
    }

    public async Task<AuthResponseDto> StaffLoginAsync(LoginDto loginDto)
    {
        try
        {
            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Email == loginDto.Email && s.IsActive);

            if (staff == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var token = GenerateJwtToken(staff.Id, staff.Email, staff.Role);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = staff.Id,
                    Name = staff.Name,
                    Email = staff.Email,
                    Role = staff.Role
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during staff login");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Login failed"
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // In a real implementation, you would validate the refresh token
        // and generate a new access token
        await Task.Delay(1); // Placeholder

        return new AuthResponseDto
        {
            Success = false,
            Message = "Refresh token functionality not implemented"
        };
    }

    private string GenerateJwtToken(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("CustomerId", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogDebug("Generated JWT Token: {Token}", tokenString);

        return tokenString;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
