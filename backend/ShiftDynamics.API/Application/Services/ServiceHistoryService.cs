using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

using ShiftDynamics.API.Application.DTOs.ServiceHistory;

public class ServiceHistoryService : IServiceHistoryService
{
    private readonly ShiftDynamicsDbContext _db;
    public ServiceHistoryService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<ServiceHistoryResponse>> GetForCustomerAsync(Guid customerId, Guid? vehicleId = null)
    {
        var query = _db.WorkOrders.AsNoTracking().Include(w => w.Vehicle).Include(w => w.Service)
            .Where(w => w.CustomerId == customerId && w.Status == WorkOrderStatus.Completed);
        if (vehicleId.HasValue) query = query.Where(w => w.VehicleId == vehicleId.Value);
        return await query.OrderByDescending(w => w.CompletedAt).Select(w => new ServiceHistoryResponse(
            w.Id,
            w.WorkOrderNumber,
            w.CompletedAt,
            w.Description,
            w.TechnicianNotes,
            new VehicleHistoryResponse(w.Vehicle.Id, w.Vehicle.RegistrationNumber, w.Vehicle.Make, w.Vehicle.Model),
            new ServiceHistoryItemResponse(w.Service.Id, w.Service.Name))).ToListAsync();
    }
}

