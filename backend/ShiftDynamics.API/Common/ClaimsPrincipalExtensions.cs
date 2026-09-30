using System.Security.Claims;

namespace ShiftDynamics.API.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid RequireUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedException();
    }

    public static Guid RequireCustomerId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("customerId");
        return Guid.TryParse(value, out var id) ? id : throw new ForbiddenException("A customer account is required.");
    }

    public static Guid? TryGetCustomerId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("customerId");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static Guid? TryGetStaffId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("staffId");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static Guid RequireStaffId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("staffId");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new ForbiddenException("A staff account is required.");
    }

    public static string? GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Role) ?? principal.FindFirstValue("role");
    }
}

