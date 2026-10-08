using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class ManagerService : IManagerService
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;
    public ManagerService(ShiftDynamicsDbContext db, INotificationService notifications) { _db = db; _notifications = notifications; }
    public async Task<object> GetDashboardAsync() => new { openJobs = await _db.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Open || w.Status == WorkOrderStatus.Assigned || w.Status == WorkOrderStatus.InProgress), completedToday = await _db.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Completed && w.CompletedAt >= DateTime.UtcNow.Date), pendingEstimates = await _db.Estimates.CountAsync(e => e.Status == EstimateStatus.Sent), availableBays = await _db.WorkshopBays.CountAsync(b => b.Status == BayStatus.Available), lowStock = await _db.InventoryItems.CountAsync(i => i.OnHandQty <= i.ReorderLevel) };
    public async Task<IReadOnlyList<WorkshopBay>> GetBaysAsync() => await _db.WorkshopBays.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
    public async Task<WorkshopBay> CreateBayAsync(string name, BayStatus status, string? notes)
    {
        var value = name.Trim(); if (await _db.WorkshopBays.AnyAsync(b => b.Name == value)) throw new ConflictException("Bay name already exists.");
        var bay = new WorkshopBay { Id = Guid.NewGuid(), Name = value, Status = status, Notes = notes?.Trim(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; _db.WorkshopBays.Add(bay); await _db.SaveChangesAsync(); return bay;
    }
    public async Task<JobAssignment> AssignJobAsync(Guid workOrderId, Guid mechanicStaffId, Guid? bayId, Guid assignedByUserId)
    {
        // Integration retry wrapper: AssignJobAsync
        return await _db.Database.CreateExecutionStrategy().ExecuteAsync<JobAssignment>(async () =>
        {
        _db.ChangeTracker.Clear();
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId) ?? throw new NotFoundException("Work order not found.");
        if (workOrder.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) throw new ConflictException("Closed work orders cannot be assigned.");
        var mechanic = await _db.Staff.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == mechanicStaffId && s.Role == SystemRole.Mechanic && s.Status == StaffStatus.Active) ?? throw new NotFoundException("Active mechanic not found.");
        if (await _db.JobAssignments.AnyAsync(a => a.MechanicStaffId == mechanicStaffId && a.IsActive && a.WorkOrderId != workOrderId)) throw new ConflictException("Mechanic already has an active assignment.");
        if (bayId.HasValue) { var bay = await _db.WorkshopBays.FirstOrDefaultAsync(b => b.Id == bayId) ?? throw new NotFoundException("Bay not found."); if ((bay.Status != BayStatus.Available && !await _db.JobAssignments.AnyAsync(a => a.BayId == bayId && a.IsActive && a.WorkOrderId == workOrderId)) || await _db.JobAssignments.AnyAsync(a => a.BayId == bayId && a.IsActive && a.WorkOrderId != workOrderId)) throw new ConflictException("Bay is not available."); bay.Status = BayStatus.Occupied; bay.UpdatedAt = DateTime.UtcNow; }
        foreach (var prior in await _db.JobAssignments.Where(a => a.WorkOrderId == workOrderId && a.IsActive).ToListAsync()) {
            prior.IsActive = false; prior.EndedAt = DateTime.UtcNow;
            if (prior.BayId.HasValue && prior.BayId != bayId)
            {
                var previousBay = await _db.WorkshopBays.FindAsync(prior.BayId.Value);
                if (previousBay != null && previousBay.Status == BayStatus.Occupied) { previousBay.Status = BayStatus.Available; previousBay.UpdatedAt = DateTime.UtcNow; }
            }
        }
        foreach (var timer in await _db.LaborSessions.Where(x => x.WorkOrderId == workOrderId && x.EndedAt == null).ToListAsync())
        { timer.EndedAt = DateTime.UtcNow; timer.Status = LaborSessionStatus.Ended; timer.DurationSeconds = Math.Max(0, (int)(timer.EndedAt.Value - timer.StartedAt).TotalSeconds - timer.PauseSeconds); }
        var assignment = new JobAssignment { Id = Guid.NewGuid(), WorkOrderId = workOrderId, MechanicStaffId = mechanicStaffId, BayId = bayId, AssignedByUserId = assignedByUserId, AssignedAt = DateTime.UtcNow, IsActive = true };
        workOrder.AssignedStaffId = mechanicStaffId; workOrder.Status = WorkOrderStatus.Assigned; workOrder.UpdatedAt = DateTime.UtcNow; _db.JobAssignments.Add(assignment); await _db.SaveChangesAsync();
        await _notifications.CreateAsync(mechanic.UserId, "assignment", "New job assignment", $"Work order {workOrder.WorkOrderNumber} has been assigned to you.", "WorkOrder", workOrder.Id); await transaction.CommitAsync(); return assignment;
    
        });
    }
    public async Task<object> GetMechanicsAsync() => await _db.Staff.AsNoTracking().Include(s => s.User).Where(s => s.Role == SystemRole.Mechanic).Select(s => new { s.Id, s.EmployeeNumber, s.User.FullName, s.Specialization, s.Status, ActiveJobs = _db.JobAssignments.Count(a => a.MechanicStaffId == s.Id && a.IsActive) }).ToListAsync();
}