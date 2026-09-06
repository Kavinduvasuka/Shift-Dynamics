using ShiftDynamics.API.DTOs.Auth;

namespace ShiftDynamics.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
