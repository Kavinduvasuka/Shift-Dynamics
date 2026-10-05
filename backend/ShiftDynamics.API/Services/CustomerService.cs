using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.DTOs.Customers;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Interfaces;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ShiftDynamicsDbContext _db;

    public CustomerService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync() =>
        (await _db.Customers.AsNoTracking().Include(c => c.Vehicles).ToListAsync()).Select(Map);

    public async Task<CustomerResponse?> GetByIdAsync(Guid id)
    {
        var customer = await _db.Customers.AsNoTracking().Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? null : Map(customer);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return false;

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Email = request.Email.Trim();
        customer.Address = request.Address.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    private static CustomerResponse Map(Customer customer) => new()
    {
        Id = customer.Id,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        Phone = customer.Phone,
        Email = customer.Email,
        Address = customer.Address,
        CreatedAt = customer.CreatedAt,
        VehicleCount = customer.Vehicles.Count
    };
}
