using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Services;

public class ModificationRequestService : IModificationRequestService
{
    private readonly ShiftDynamicsDbContext _db;
    public ModificationRequestService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<ModificationRequest> CreateAsync(Guid customerId, Guid vehicleId, string requestType, string description)
    {
        if (!await _db.Vehicles.AnyAsync(v => v.Id == vehicleId && v.CustomerId == customerId)) throw new NotFoundException("Vehicle not found.");
        var request = new ModificationRequest { Id = Guid.NewGuid(), CustomerId = customerId, VehicleId = vehicleId, RequestType = requestType.Trim(), Description = description.Trim(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.ModificationRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<IReadOnlyList<ModificationRequest>> ListAsync(Guid? customerId, ModificationRequestStatus? status)
    {
        var query = _db.ModificationRequests.AsNoTracking().Include(r => r.Vehicle).AsQueryable();
        if (customerId.HasValue) query = query.Where(r => r.CustomerId == customerId);
        if (status.HasValue) query = query.Where(r => r.Status == status);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<ModificationRequest?> ReviewAsync(Guid id, ModificationRequestStatus status, decimal? proposedCost, string? advisorNotes)
    {
        if (proposedCost < 0) throw new ValidationException("Proposed cost cannot be negative.");
        var request = await _db.ModificationRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null) return null;
        if (request.Status is ModificationRequestStatus.Approved or ModificationRequestStatus.Rejected or ModificationRequestStatus.Cancelled) throw new ConflictException("This request is already closed.");
        request.Status = status; request.ProposedCost = proposedCost; request.AdvisorNotes = advisorNotes?.Trim(); request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return request;
    }
}
