using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;
using ShiftDynamics.API.Application.DTOs.Mechanic;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/mechanic")]
[Authorize(Policy = "Mechanic")]
public class MechanicController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly IMechanicService _mechanic;

    public MechanicController(ShiftDynamicsDbContext db, IMechanicService mechanic) { _db = db; _mechanic = mechanic; }

    private async Task<Guid> CurrentMechanicId()
    {
        var userId = User.RequireUserId();

        return await _mechanic.GetCurrentMechanicIdAsync(userId);
    }

    private async Task<WorkOrder> GetAssignedWorkOrder(Guid mechanicId, Guid workOrderId)
    {
        var assigned = await _db.JobAssignments.AnyAsync(a =>
            a.WorkOrderId == workOrderId &&
            a.MechanicStaffId == mechanicId &&
            a.IsActive);

        if (!assigned)
            throw new ForbiddenException("This job is not assigned to you.");

        return await _db.WorkOrders
            .Include(w => w.Vehicle)
            .Include(w => w.Customer)
            .Include(w => w.Service)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new NotFoundException("Work order not found.");
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<ApiResponse<object>>> MyJobs()
    {
        var mechanicId = await CurrentMechanicId();

        var jobs = await _db.JobAssignments
            .AsNoTracking()
            .Include(a => a.WorkOrder).ThenInclude(w => w.Vehicle)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Customer)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Service)
            .Where(a => a.IsActive && a.MechanicStaffId == mechanicId)
            .Select(a => new
            {
                a.Id,
                a.WorkOrderId,
                a.WorkOrder.WorkOrderNumber,
                a.WorkOrder.Status,
                a.WorkOrder.Description,
                Vehicle = a.WorkOrder.Vehicle.RegistrationNumber,
                Customer = a.WorkOrder.Customer.FirstName + " " + a.WorkOrder.Customer.LastName,
                Service = a.WorkOrder.Service.Name,
                a.BayId,
                a.AssignedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(jobs));
    }

    [HttpGet("jobs/{workOrderId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> JobDetails(Guid workOrderId)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder = await GetAssignedWorkOrder(mechanicId, workOrderId);

        var assignment = await _db.JobAssignments.AsNoTracking()
            .Where(a => a.WorkOrderId == workOrderId &&
                        a.MechanicStaffId == mechanicId &&
                        a.IsActive)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();

        var diagnostics = await _db.DiagnosticFindings.AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var repairs = await _db.RepairActions.AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var recommendations = await _db.MechanicRecommendations.AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var requisitions = await _db.PartRequisitions.AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId &&
                        x.RequestedByStaffId == mechanicId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var laborSessions = await _db.LaborSessions.AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId &&
                        x.MechanicStaffId == mechanicId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            WorkOrder = new
            {
                workOrder.Id,
                workOrder.WorkOrderNumber,
                workOrder.Status,
                workOrder.Description,
                workOrder.TechnicianNotes,
                workOrder.StartedAt,
                workOrder.CompletedAt,
                workOrder.CreatedAt,
                workOrder.UpdatedAt,
                Vehicle = new
                {
                    workOrder.Vehicle.Id,
                    workOrder.Vehicle.RegistrationNumber,
                    workOrder.Vehicle.Make,
                    workOrder.Vehicle.Model,
                    workOrder.Vehicle.Year
                },
                Customer = new
                {
                    workOrder.Customer.Id,
                    Name = workOrder.Customer.FirstName + " " +
                           workOrder.Customer.LastName
                },
                Service = new
                {
                    workOrder.Service.Id,
                    workOrder.Service.Name
                }
            },
            Assignment = assignment,
            Diagnostics = diagnostics,
            Repairs = repairs,
            Recommendations = recommendations,
            Requisitions = requisitions,
            LaborSessions = laborSessions
        }));
    }

    [HttpPost("timer/start")]
    public async Task<ActionResult<ApiResponse<object>>> StartTimer(
        [FromBody] TimerActionRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var session = await _mechanic.StartTimerAsync(mechanicId, request.WorkOrderId);

        return Ok(ApiResponse<object>.Ok(session, "Timer started."));
    }

    [HttpPost("timer/end")]
    public async Task<ActionResult<ApiResponse<object>>> EndTimer(
        [FromBody] TimerActionRequest request)
    {
        var mechanicId = await CurrentMechanicId();

        var session = await _mechanic.EndTimerAsync(mechanicId, request.WorkOrderId);

        return Ok(ApiResponse<object>.Ok(session, "Timer ended."));
    }

    [HttpPost("diagnostics")]
    public async Task<ActionResult<ApiResponse<object>>> AddDiagnostic(
        [FromBody] CreateDiagnosticRequest request)
    {
        var item = await _mechanic.AddDiagnosticAsync(await CurrentMechanicId(), request.WorkOrderId, request.Finding, request.Severity);

        return Ok(ApiResponse<object>.Ok(
            item,
            "Diagnostic finding recorded."));
    }

    [HttpPost("repairs")]
    public async Task<ActionResult<ApiResponse<object>>> AddRepair(
        [FromBody] CreateRepairRequest request)
    {
        var item = await _mechanic.AddRepairAsync(await CurrentMechanicId(), request.WorkOrderId, request.Action, request.Notes);

        return Ok(ApiResponse<object>.Ok(
            item,
            "Repair action recorded."));
    }

    [HttpPost("recommendations")]
    public async Task<ActionResult<ApiResponse<object>>> AddRecommendation(
        [FromBody] CreateRecommendationRequest request)
    {
        var item = await _mechanic.AddRecommendationAsync(await CurrentMechanicId(), request.WorkOrderId, request.Recommendation, request.Priority);

        return Ok(ApiResponse<object>.Ok(
            item,
            "Recommendation recorded."));
    }

    [HttpPatch("jobs/{workOrderId:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateJobStatus(
        Guid workOrderId,
        [FromBody] UpdateJobStatusRequest request)
    {
        var workOrder = await _mechanic.UpdateJobStatusAsync(await CurrentMechanicId(), workOrderId, request.Status, request.Notes);

        return Ok(ApiResponse<object>.Ok(
            workOrder,
            "Job status updated."));
    }

    [HttpPost("jobs/{workOrderId:guid}/complete")]
    public async Task<ActionResult<ApiResponse<object>>> CompleteJob(
        Guid workOrderId)
    {
        var workOrder = await _mechanic.CompleteJobAsync(await CurrentMechanicId(), workOrderId);
        return Ok(ApiResponse<object>.Ok(workOrder, "Job completed."));
    }

    private async Task<ActionResult<ApiResponse<object>>> CompleteJobInternal(
        Guid mechanicId,
        WorkOrder workOrder,
        string? notes)
    {
        var now = DateTime.UtcNow;

        var activeSession = await _db.LaborSessions
            .Where(s =>
                s.MechanicStaffId == mechanicId &&
                s.WorkOrderId == workOrder.Id &&
                (s.Status == LaborSessionStatus.Active ||
                 s.Status == LaborSessionStatus.Paused))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        if (activeSession is not null)
        {
            activeSession.EndedAt = now;
            activeSession.Status = LaborSessionStatus.Ended;

            activeSession.DurationSeconds = Math.Max(
                0,
                (int)(now - activeSession.StartedAt).TotalSeconds -
                activeSession.PauseSeconds);
        }

        var assignment = await _db.JobAssignments
            .Where(a =>
                a.WorkOrderId == workOrder.Id &&
                a.MechanicStaffId == mechanicId &&
                a.IsActive)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();

        if (assignment is null)
            throw new ForbiddenException(
                "This job is not actively assigned to you.");

        assignment.IsActive = false;
        assignment.EndedAt = now;

        workOrder.Status = WorkOrderStatus.Completed;
        workOrder.CompletedAt = now;

        if (!string.IsNullOrWhiteSpace(notes))
            workOrder.TechnicianNotes = notes.Trim();

        workOrder.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            workOrder,
            "Job completed."));
    }

    [HttpGet("completed-jobs")]
    public async Task<ActionResult<ApiResponse<object>>> CompletedJobs()
    {
        var mechanicId = await CurrentMechanicId();

        var jobs = await _db.JobAssignments
            .AsNoTracking()
            .Where(a =>
                a.MechanicStaffId == mechanicId &&
                !a.IsActive &&
                a.WorkOrder.Status == WorkOrderStatus.Completed)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Vehicle)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Customer)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Service)
            .OrderByDescending(a => a.EndedAt)
            .Select(a => new
            {
                a.Id,
                a.WorkOrderId,
                a.WorkOrder.WorkOrderNumber,
                a.WorkOrder.Status,
                a.WorkOrder.CompletedAt,
                Vehicle = a.WorkOrder.Vehicle.RegistrationNumber,
                Customer = a.WorkOrder.Customer.FirstName + " " +
                           a.WorkOrder.Customer.LastName,
                Service = a.WorkOrder.Service.Name,
                a.EndedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(jobs));
    }

    [HttpPost("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRequisition(
        [FromBody] CreateRequisitionRequest request)
    {
        var req = await _mechanic.CreateRequisitionAsync(await CurrentMechanicId(), request.WorkOrderId, request.PartId, request.PartSpec, request.QtyRequested, request.Urgency, request.Reason);

        return Ok(ApiResponse<object>.Ok(
            req,
            "Requisition submitted."));
    }
}

