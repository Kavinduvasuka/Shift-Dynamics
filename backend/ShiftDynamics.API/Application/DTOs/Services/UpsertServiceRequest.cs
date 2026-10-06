namespace ShiftDynamics.API.Application.DTOs.Services;

public record UpsertServiceRequest(string Name, string? Description, decimal BasePrice, int EstimatedDurationMinutes, bool IsActive = true);
