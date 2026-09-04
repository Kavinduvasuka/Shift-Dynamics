using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Policy = "Manager")]
public class ManagerController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public ManagerController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> Dashboard()
    {
        var openJobs = await _db.WorkOrders.CountAsync(w =>
            w.Status == WorkOrderStatus.Open || w.Status == WorkOrderStatus.Assigned || w.Status == WorkOrderStatus.InProgress);
        var completedToday = await _db.WorkOrders.CountAsync(w =>
            w.Status == WorkOrderStatus.Completed && w.CompletedAt >= DateTime.UtcNow.Date);
        var pendingEstimates = await _db.Estimates.CountAsync(e => e.Status == EstimateStatus.Sent);
        var availableBays = await _db.WorkshopBays.CountAsync(b => b.Status == BayStatus.Available);
        var lowStock = await _db.InventoryItems.CountAsync(i => i.OnHandQty <= i.ReorderLevel);

        return Ok(ApiResponse<object>.Ok(new
        {
            openJobs,
            completedToday,
            pendingEstimates,
            availableBays,
            lowStock
        }));
    }

    [HttpGet("bays")]
    public async Task<ActionResult<ApiResponse<object>>> Bays()
    {
        var bays = await _db.WorkshopBays.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        return Ok(ApiResponse<object>.Ok(bays));
    }

    public record UpsertBayRequest(string Name, BayStatus Status, string? Notes);

    [HttpPost("bays")]
    public async Task<ActionResult<ApiResponse<object>>> CreateBay([FromBody] UpsertBayRequest request)
    {
        if (await _db.WorkshopBays.AnyAsync(b => b.Name == request.Name.Trim()))
            throw new ConflictException("Bay name already exists.");

        var bay = new WorkshopBay
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Status = request.Status,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.WorkshopBays.Add(bay);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(bay));
    }

    public record AssignJobRequest(Guid WorkOrderId, Guid MechanicStaffId, Guid? BayId);

    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponse<object>>> Assign([FromBody] AssignJobRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var wo = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId)
            ?? throw new NotFoundException("Work order not found.");

        var mechanic = await _db.Staff.FirstOrDefaultAsync(s =>
            s.Id == request.MechanicStaffId && s.Role == SystemRole.Mechanic && s.Status == StaffStatus.Active)
            ?? throw new NotFoundException("Active mechanic not found.");

        var activeMechanicAssignment = await _db.JobAssignments
            .AnyAsync(a => a.MechanicStaffId == request.MechanicStaffId && a.IsActive);
        if (activeMechanicAssignment)
            throw new ConflictException("Mechanic already has an active assignment.");

        if (request.BayId.HasValue)
        {
            var bay = await _db.WorkshopBays.FirstOrDefaultAsync(b => b.Id == request.BayId)
                ?? throw new NotFoundException("Bay not found.");

            if (bay.Status != BayStatus.Available)
                throw new ConflictException("Bay is not available.");

            var bayBusy = await _db.JobAssignments.AnyAsync(a => a.BayId == request.BayId && a.IsActive);
            if (bayBusy)
                throw new ConflictException("Bay is already occupied.");

            bay.Status = BayStatus.Occupied;
            bay.UpdatedAt = DateTime.UtcNow;
        }

        // End any previous assignment on this work order
        var previous = await _db.JobAssignments
            .Where(a => a.WorkOrderId == request.WorkOrderId && a.IsActive)
            .ToListAsync();
        foreach (var p in previous)
        {
            p.IsActive = false;
            p.EndedAt = DateTime.UtcNow;
        }

        var assignment = new JobAssignment
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = request.MechanicStaffId,
            BayId = request.BayId,
            AssignedByUserId = User.RequireUserId(),
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        wo.AssignedStaffId = request.MechanicStaffId;
        wo.Status = WorkOrderStatus.Assigned;
        wo.UpdatedAt = DateTime.UtcNow;

        _db.JobAssignments.Add(assignment);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(ApiResponse<object>.Ok(assignment, "Job assigned."));
    }

    [HttpGet("mechanics")]
    public async Task<ActionResult<ApiResponse<object>>> Mechanics()
    {
        var mechanics = await _db.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.Role == SystemRole.Mechanic)
            .Select(s => new
            {
                s.Id,
                s.EmployeeNumber,
                s.User.FullName,
                s.Specialization,
                s.Status,
                ActiveJobs = _db.JobAssignments.Count(a => a.MechanicStaffId == s.Id && a.IsActive)
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(mechanics));
    }
}
