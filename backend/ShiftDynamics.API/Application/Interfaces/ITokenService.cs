using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);
}

