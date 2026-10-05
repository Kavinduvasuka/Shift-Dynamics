using ShiftDynamics.API.Application.DTOs.Customers;

namespace ShiftDynamics.API.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request);
}

