using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.DTOs.Auth;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Services;
using System.Security.Claims;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ShiftDynamicsDbContext _db;

    public AuthController(IAuthService authService, ShiftDynamicsDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    /// <summary>Customer self-registration.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterCustomerAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
    }

    /// <summary>Unified login for customers and staff.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    /// <summary>Current authenticated user profile.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId,
            AccessToken = string.Empty,
            ExpiresAt = DateTime.UtcNow
        }));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.RequestPasswordResetAsync(request.Email);
        return Ok(ApiResponse.Ok("If an account exists for that email, a reset link has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.Ok("Password has been reset successfully."));
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponse> Logout()
    {
        // JWT is stateless; client discards the token.
        // Future: add refresh-token revocation table if needed.
        return Ok(ApiResponse.Ok("Logged out."));
    }
}
