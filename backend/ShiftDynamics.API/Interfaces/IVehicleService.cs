using ShiftDynamics.API.DTOs.Vehicles;

namespace ShiftDynamics.API.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<VehicleResponse>> GetAllAsync(Guid? customerId = null);
    Task<VehicleResponse?> GetByIdAsync(Guid id, Guid? customerId = null);
    Task<VehicleResponse> CreateAsync(Guid customerId, CreateVehicleRequest request);
    Task<VehicleResponse?> UpdateAsync(Guid id, Guid customerId, UpdateVehicleRequest request);
    Task<bool> DeleteAsync(Guid id, Guid customerId);
}
