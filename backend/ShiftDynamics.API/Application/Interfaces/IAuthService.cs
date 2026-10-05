using ShiftDynamics.API.Application.DTOs.Auth;

namespace ShiftDynamics.API.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}

