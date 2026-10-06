namespace ShiftDynamics.API.Application.Interfaces;

using ShiftDynamics.API.Application.DTOs.ServiceHistory;

public interface IServiceHistoryService
{
    Task<IReadOnlyList<ServiceHistoryResponse>> GetForCustomerAsync(Guid customerId, Guid? vehicleId = null);
}

