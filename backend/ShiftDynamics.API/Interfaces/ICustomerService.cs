using ShiftDynamics.API.DTOs.Customers;

namespace ShiftDynamics.API.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request);
}
