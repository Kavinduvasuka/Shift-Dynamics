using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class ServiceHistoryService : IServiceHistoryService
{
    private readonly ShiftDynamicsDbContext _db;
    public ServiceHistoryService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<object>> GetForCustomerAsync(Guid customerId, Guid? vehicleId = null)
    {
        var query = _db.WorkOrders.AsNoTracking().Include(w => w.Vehicle).Include(w => w.Service)
            .Where(w => w.CustomerId == customerId && w.Status == WorkOrderStatus.Completed);
        if (vehicleId.HasValue) query = query.Where(w => w.VehicleId == vehicleId.Value);
        return await query.OrderByDescending(w => w.CompletedAt).Select(w => (object)new
        {
            w.Id, w.WorkOrderNumber, w.CompletedAt, w.Description, w.TechnicianNotes,
            Vehicle = new { w.Vehicle.Id, w.Vehicle.RegistrationNumber, w.Vehicle.Make, w.Vehicle.Model },
            Service = new { w.Service.Id, w.Service.Name }
        }).ToListAsync();
    }
}

