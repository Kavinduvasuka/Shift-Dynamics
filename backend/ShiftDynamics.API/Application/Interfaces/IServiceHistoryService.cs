namespace ShiftDynamics.API.Application.Interfaces;

public interface IServiceHistoryService
{
    Task<IReadOnlyList<object>> GetForCustomerAsync(Guid customerId, Guid? vehicleId = null);
}

