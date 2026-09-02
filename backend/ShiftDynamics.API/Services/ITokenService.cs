using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);
}
