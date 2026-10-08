using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Application.DTOs.Customers;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Services;

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

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.CustomerId == id);

        if (user is null)
            throw new ShiftDynamics.API.Common.ConflictException(
                "This customer has no linked login account.");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var fullName = (firstName + " " + lastName).Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();

        if (fullName.Length > 150)
            throw new ShiftDynamics.API.Common.ValidationException(
                "Full name cannot exceed 150 characters.");

        if (email.Length > 191)
            throw new ShiftDynamics.API.Common.ValidationException(
                "Email cannot exceed 191 characters.");

        if (await _db.Users.AnyAsync(u =>
            u.Id != user.Id && u.Email == email))
            throw new ShiftDynamics.API.Common.ConflictException(
                "This email is already used by another account.");

        if (await _db.Users.AnyAsync(u =>
            u.Id != user.Id && u.Phone == phone))
            throw new ShiftDynamics.API.Common.ConflictException(
                "This phone number is already used by another account.");

        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = request.Address?.Trim() ?? string.Empty;

        user.FullName = fullName;
        user.Email = email;
        user.Phone = phone;
        user.UpdatedAt = DateTime.UtcNow;

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

