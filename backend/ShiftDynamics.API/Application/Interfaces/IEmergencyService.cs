using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

public interface IEmergencyService
{
    Task<IReadOnlyList<EmergencyServiceProvider>> FindProvidersAsync(decimal? latitude, decimal? longitude, string? category, double radiusKm);
    Task<EmergencyRequest> CreateRequestAsync(Guid customerId, Guid? vehicleId, string location, decimal? latitude, decimal? longitude, string problemDescription);
    Task<IReadOnlyList<EmergencyRequest>> ListRequestsAsync(EmergencyRequestStatus? status);
}

