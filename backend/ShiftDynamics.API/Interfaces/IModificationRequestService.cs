using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface IModificationRequestService
{
    Task<ModificationRequest> CreateAsync(Guid customerId, Guid vehicleId, string requestType, string description);
    Task<IReadOnlyList<ModificationRequest>> ListAsync(Guid? customerId, ModificationRequestStatus? status);
    Task<ModificationRequest?> ReviewAsync(Guid id, ModificationRequestStatus status, decimal? proposedCost, string? advisorNotes);
}
