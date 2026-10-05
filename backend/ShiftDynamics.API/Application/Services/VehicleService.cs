using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Application.DTOs.Vehicles;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly ShiftDynamicsDbContext _db;
    public VehicleService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IEnumerable<VehicleResponse>> GetAllAsync(Guid? customerId = null)
    {
        var query = _db.Vehicles.AsNoTracking().AsQueryable();
        if (customerId.HasValue) query = query.Where(v => v.CustomerId == customerId);
        return (await query.OrderByDescending(v => v.CreatedAt).ToListAsync()).Select(Map);
    }

    public async Task<VehicleResponse?> GetByIdAsync(Guid id, Guid? customerId = null)
    {
        var query = _db.Vehicles.AsNoTracking().Where(v => v.Id == id);
        if (customerId.HasValue) query = query.Where(v => v.CustomerId == customerId);
        var vehicle = await query.FirstOrDefaultAsync();
        return vehicle is null ? null : Map(vehicle);
    }

    public async Task<VehicleResponse> CreateAsync(Guid customerId, CreateVehicleRequest request)
    {
        var vehicle = new Vehicle { Id = Guid.NewGuid(), CustomerId = customerId, RegistrationNumber = request.RegistrationNumber.Trim(), Make = request.Make.Trim(), Model = request.Model.Trim(), Year = request.Year, VIN = string.IsNullOrWhiteSpace(request.VIN) ? null : request.VIN.Trim(), Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(), CreatedAt = DateTime.UtcNow };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return Map(vehicle);
    }

    public async Task<VehicleResponse?> UpdateAsync(Guid id, Guid customerId, UpdateVehicleRequest request)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == customerId);
        if (vehicle is null) return null;
        vehicle.RegistrationNumber = request.RegistrationNumber.Trim(); vehicle.Make = request.Make.Trim(); vehicle.Model = request.Model.Trim(); vehicle.Year = request.Year; vehicle.VIN = string.IsNullOrWhiteSpace(request.VIN) ? null : request.VIN.Trim(); vehicle.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        await _db.SaveChangesAsync();
        return Map(vehicle);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid customerId)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == customerId);
        if (vehicle is null) return false;
        if (await _db.Appointments.AnyAsync(a => a.VehicleId == id) || await _db.WorkOrders.AnyAsync(w => w.VehicleId == id)) throw new ConflictException("Vehicles with service history cannot be deleted.");
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return true;
    }

    private static VehicleResponse Map(Vehicle vehicle) => new() { Id = vehicle.Id, CustomerId = vehicle.CustomerId, RegistrationNumber = vehicle.RegistrationNumber, Make = vehicle.Make, Model = vehicle.Model, Year = vehicle.Year, VIN = vehicle.VIN, Color = vehicle.Color, CreatedAt = vehicle.CreatedAt };
}

