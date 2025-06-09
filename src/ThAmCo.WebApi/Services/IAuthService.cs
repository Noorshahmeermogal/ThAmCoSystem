using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> StaffLoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
} 