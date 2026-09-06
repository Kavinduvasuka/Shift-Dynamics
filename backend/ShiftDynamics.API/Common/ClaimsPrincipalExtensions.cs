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
}