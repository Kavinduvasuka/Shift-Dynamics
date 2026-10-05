namespace ShiftDynamics.API.Interfaces;

public interface IServiceHistoryService
{
    Task<IReadOnlyList<object>> GetForCustomerAsync(Guid customerId, Guid? vehicleId = null);
}
