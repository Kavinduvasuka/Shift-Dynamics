using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Services;

public class MechanicService : IMechanicService
{
    private readonly ShiftDynamicsDbContext _db;
    public MechanicService(ShiftDynamicsDbContext db) => _db = db;
    public async Task<Guid> GetCurrentMechanicIdAsync(Guid userId) => (await _db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId && s.Role == SystemRole.Mechanic && s.Status == StaffStatus.Active))?.Id ?? throw new ForbiddenException("Active mechanic profile is required.");
    public async Task<LaborSession> StartTimerAsync(Guid mechanicId, Guid workOrderId)
    {
        var workOrder = await AssignedAsync(mechanicId, workOrderId); if (workOrder.Status is not (WorkOrderStatus.Assigned or WorkOrderStatus.InProgress or WorkOrderStatus.WaitingForParts)) throw new ConflictException("This job cannot be started in its current status.");
        if (await _db.LaborSessions.AnyAsync(s => s.MechanicStaffId == mechanicId && s.Status == LaborSessionStatus.Active)) throw new ConflictException("Mechanic already has an active timer.");
        var now = DateTime.UtcNow; var session = new LaborSession { Id = Guid.NewGuid(), WorkOrderId = workOrderId, MechanicStaffId = mechanicId, StartedAt = now, Status = LaborSessionStatus.Active }; workOrder.Status = WorkOrderStatus.InProgress; workOrder.StartedAt ??= now; workOrder.UpdatedAt = now; _db.LaborSessions.Add(session); await _db.SaveChangesAsync(); return session;
    }
    public async Task<LaborSession> EndTimerAsync(Guid mechanicId, Guid workOrderId)
    {
        await AssignedAsync(mechanicId, workOrderId); var session = await _db.LaborSessions.Where(s => s.MechanicStaffId == mechanicId && s.WorkOrderId == workOrderId && (s.Status == LaborSessionStatus.Active || s.Status == LaborSessionStatus.Paused)).OrderByDescending(s => s.StartedAt).FirstOrDefaultAsync() ?? throw new NotFoundException("No active labor session found.");
        session.EndedAt = DateTime.UtcNow; session.Status = LaborSessionStatus.Ended; session.DurationSeconds = Math.Max(0, (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds - session.PauseSeconds); await _db.SaveChangesAsync(); return session;
    }
    public async Task<DiagnosticFinding> AddDiagnosticAsync(Guid mechanicId, Guid workOrderId, string finding, string? severity)
    { await EnsureOpenAsync(mechanicId, workOrderId); if (string.IsNullOrWhiteSpace(finding)) throw new ValidationException("Finding is required."); var item = new DiagnosticFinding { Id = Guid.NewGuid(), WorkOrderId = workOrderId, MechanicStaffId = mechanicId, Finding = finding.Trim(), Severity = severity?.Trim() }; _db.DiagnosticFindings.Add(item); await _db.SaveChangesAsync(); return item; }
    public async Task<RepairAction> AddRepairAsync(Guid mechanicId, Guid workOrderId, string action, string? notes)
    { await EnsureOpenAsync(mechanicId, workOrderId); if (string.IsNullOrWhiteSpace(action)) throw new ValidationException("Repair action is required."); var item = new RepairAction { Id = Guid.NewGuid(), WorkOrderId = workOrderId, MechanicStaffId = mechanicId, Action = action.Trim(), Notes = notes?.Trim() }; _db.RepairActions.Add(item); await _db.SaveChangesAsync(); return item; }
    public async Task<MechanicRecommendation> AddRecommendationAsync(Guid mechanicId, Guid workOrderId, string recommendation, string? priority)
    { await EnsureOpenAsync(mechanicId, workOrderId); if (string.IsNullOrWhiteSpace(recommendation)) throw new ValidationException("Recommendation is required."); var item = new MechanicRecommendation { Id = Guid.NewGuid(), WorkOrderId = workOrderId, MechanicStaffId = mechanicId, Recommendation = recommendation.Trim(), Priority = priority?.Trim() }; _db.MechanicRecommendations.Add(item); await _db.SaveChangesAsync(); return item; }
    public async Task<PartRequisition> CreateRequisitionAsync(Guid mechanicId, Guid workOrderId, Guid? partId, string partSpec, int quantity, RequisitionUrgency urgency, string? reason)
    { await EnsureOpenAsync(mechanicId, workOrderId); if (quantity <= 0 || string.IsNullOrWhiteSpace(partSpec)) throw new ValidationException("Part specification and a positive quantity are required."); if (partId.HasValue && !await _db.Parts.AnyAsync(p => p.Id == partId.Value)) throw new NotFoundException("Part not found."); var item = new PartRequisition { Id = Guid.NewGuid(), WorkOrderId = workOrderId, RequestedByStaffId = mechanicId, PartId = partId, PartSpec = partSpec.Trim(), QtyRequested = quantity, Urgency = urgency, Reason = reason?.Trim(), Status = RequisitionStatus.Pending, CreatedAt = DateTime.UtcNow }; _db.PartRequisitions.Add(item); await _db.SaveChangesAsync(); return item; }
    private async Task<WorkOrder> AssignedAsync(Guid mechanicId, Guid workOrderId) { if (!await _db.JobAssignments.AnyAsync(a => a.WorkOrderId == workOrderId && a.MechanicStaffId == mechanicId && a.IsActive)) throw new ForbiddenException("This job is not assigned to you."); return await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId) ?? throw new NotFoundException("Work order not found."); }
    private async Task EnsureOpenAsync(Guid mechanicId, Guid workOrderId) { var workOrder = await AssignedAsync(mechanicId, workOrderId); if (workOrder.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) throw new ConflictException("This job is closed."); }
    public async Task<WorkOrder> UpdateJobStatusAsync(Guid mechanicId, Guid workOrderId, WorkOrderStatus status, string? notes)
    {
        var workOrder = await AssignedAsync(mechanicId, workOrderId); if (workOrder.Status == status) return workOrder;
        var allowed = (workOrder.Status, status) switch { (WorkOrderStatus.Assigned, WorkOrderStatus.InProgress) => true, (WorkOrderStatus.InProgress, WorkOrderStatus.WaitingForParts) => true, (WorkOrderStatus.WaitingForParts, WorkOrderStatus.InProgress) => true, (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true, _ => false };
        if (!allowed) throw new ConflictException($"Cannot change job status from {workOrder.Status} to {status}.");
        if (status == WorkOrderStatus.Completed) return await CompleteJobAsync(mechanicId, workOrderId, notes);
        workOrder.Status = status; workOrder.StartedAt ??= status == WorkOrderStatus.InProgress ? DateTime.UtcNow : null; if (!string.IsNullOrWhiteSpace(notes)) workOrder.TechnicianNotes = notes.Trim(); workOrder.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return workOrder;
    }
    public async Task<WorkOrder> CompleteJobAsync(Guid mechanicId, Guid workOrderId, string? notes = null)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(); var workOrder = await AssignedAsync(mechanicId, workOrderId); if (workOrder.Status != WorkOrderStatus.InProgress) throw new ConflictException("Only an in-progress job can be completed.");
        var now = DateTime.UtcNow; var session = await _db.LaborSessions.Where(s => s.MechanicStaffId == mechanicId && s.WorkOrderId == workOrderId && (s.Status == LaborSessionStatus.Active || s.Status == LaborSessionStatus.Paused)).OrderByDescending(s => s.StartedAt).FirstOrDefaultAsync(); if (session is not null) { session.EndedAt = now; session.Status = LaborSessionStatus.Ended; session.DurationSeconds = Math.Max(0, (int)(now - session.StartedAt).TotalSeconds - session.PauseSeconds); }
        var assignment = await _db.JobAssignments.FirstAsync(a => a.WorkOrderId == workOrderId && a.MechanicStaffId == mechanicId && a.IsActive); assignment.IsActive = false; assignment.EndedAt = now;
        if (assignment.BayId.HasValue) { var bay = await _db.WorkshopBays.FindAsync(assignment.BayId.Value); if (bay is not null) { bay.Status = BayStatus.Available; bay.UpdatedAt = now; } }
        workOrder.Status = WorkOrderStatus.Completed; workOrder.CompletedAt = now; workOrder.UpdatedAt = now; if (!string.IsNullOrWhiteSpace(notes)) workOrder.TechnicianNotes = notes.Trim(); await _db.SaveChangesAsync(); await transaction.CommitAsync(); return workOrder;
    }
}
