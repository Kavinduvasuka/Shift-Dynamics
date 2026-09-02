using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.DTOs.Auth;
using ShiftDynamics.API.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace ShiftDynamics.API.Services;

public class AuthService : IAuthService
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ShiftDynamicsDbContext db,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("An account with this email already exists.");

        if (await _db.Users.AnyAsync(u => u.Phone == phone))
            throw new ConflictException("An account with this phone number already exists.");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FullName.Split(' ', 2)[0],
            LastName = request.FullName.Contains(' ')
                ? request.FullName[(request.FullName.IndexOf(' ') + 1)..]
                : string.Empty,
            Email = email,
            Phone = phone,
            Address = request.Address?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = SystemRole.Customer,
            Status = AccountStatus.Active,
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expires) = _tokenService.CreateAccessToken(user);
        return MapAuthResponse(user, token, expires);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (user.Status == AccountStatus.Inactive)
            throw new ForbiddenException("This account is inactive.");

        if (user.Status == AccountStatus.Pending)
            throw new ForbiddenException("This account is pending approval.");

        if (user.Status == AccountStatus.Rejected)
            throw new ForbiddenException("This account registration was rejected.");

        var (token, expires) = _tokenService.CreateAccessToken(user);
        return MapAuthResponse(user, token, expires);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Always return success to avoid email enumeration
        if (user is null)
        {
            _logger.LogInformation("Password reset requested for unknown email {Email}", email);
            return;
        }

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = HashToken(rawToken);

        var reset = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetTokens.Add(reset);
        await _db.SaveChangesAsync();

        // Email provider not configured yet – log token for development only
        _logger.LogWarning(
            "DEV ONLY – Password reset token for {Email}: {Token} (expires {Expires})",
            email, rawToken, reset.ExpiresAt);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tokenHash = HashToken(request.Token);

        var reset = await _db.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.TokenHash == tokenHash && t.User.Email == email)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (reset is null || reset.UsedAt is not null || reset.ExpiresAt < DateTime.UtcNow)
            throw new ValidationException("Invalid or expired password reset token.");

        reset.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        reset.User.UpdatedAt = DateTime.UtcNow;
        reset.UsedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    private static AuthResponse MapAuthResponse(User user, string token, DateTime expires) => new()
    {
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role.ToString(),
        Status = user.Status.ToString(),
        CustomerId = user.CustomerId,
        AccessToken = token,
        ExpiresAt = expires
    };
}
