using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.ServiceHistory;

namespace ShiftDynamics.API.Application.Services;

public class ServiceHistoryService : IServiceHistoryService
{
    private readonly ShiftDynamicsDbContext _db;

    public ServiceHistoryService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<ServiceHistoryResponse>> GetForCustomerAsync(
        Guid customerId, Guid? vehicleId = null)
    {
        // Only this customer's completed jobs can supply workshop records.
        var query = _db.WorkOrders.AsNoTracking()
            .Where(w =>
                w.CustomerId == customerId &&
                w.Status == WorkOrderStatus.Completed);

        if (vehicleId.HasValue)
            query = query.Where(w => w.VehicleId == vehicleId.Value);

        var history = await query
            .OrderByDescending(w => w.CompletedAt)
            .Select(w => new ServiceHistoryResponse(
                w.Id,
                w.WorkOrderNumber,
                w.CompletedAt,
                w.Description,
                w.TechnicianNotes,
                new VehicleHistoryResponse(
                    w.Vehicle.Id,
                    w.Vehicle.RegistrationNumber,
                    w.Vehicle.Make,
                    w.Vehicle.Model),
                new ServiceHistoryItemResponse(w.Service.Id, w.Service.Name)))
            .ToListAsync();

        if (history.Count == 0) return history;

        var jobIds = history.Select(w => w.Id).ToArray();

        var diagnostics = await _db.DiagnosticFindings.AsNoTracking()
            .Where(d => jobIds.Contains(d.WorkOrderId))
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DiagnosticHistoryResponse(
                d.Id, d.WorkOrderId, d.Finding, d.Severity, d.CreatedAt))
            .ToListAsync();

        var repairs = await _db.RepairActions.AsNoTracking()
            .Where(r => jobIds.Contains(r.WorkOrderId))
            .OrderBy(r => r.CreatedAt)
            .Select(r => new RepairHistoryResponse(
                r.Id, r.WorkOrderId, r.Action, r.Notes, r.CreatedAt))
            .ToListAsync();

        var recommendations = await _db.MechanicRecommendations.AsNoTracking()
            .Where(r => jobIds.Contains(r.WorkOrderId))
            .OrderBy(r => r.CreatedAt)
            .Select(r => new RecommendationHistoryResponse(
                r.Id, r.WorkOrderId, r.Recommendation, r.Priority, r.CreatedAt))
            .ToListAsync();

        var findingsByJob = diagnostics.ToLookup(d => d.WorkOrderId);
        var repairsByJob = repairs.ToLookup(r => r.WorkOrderId);
        var recommendationsByJob = recommendations.ToLookup(r => r.WorkOrderId);

        foreach (var item in history)
        {
            item.Diagnostics = findingsByJob[item.Id].ToArray();
            item.Repairs = repairsByJob[item.Id].ToArray();
            item.Recommendations = recommendationsByJob[item.Id].ToArray();
        }

        return history;
    }
}