using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/mechanic")]
[Authorize(Policy = "Mechanic")]
public class MechanicController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public MechanicController(ShiftDynamicsDbContext db) => _db = db;

    private async Task<Guid> CurrentMechanicId()
    {
        var userId = User.RequireUserId();

        return (await _db.Staff.AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.Role == SystemRole.Mechanic &&
                s.Status == StaffStatus.Active))?.Id
            ?? throw new ForbiddenException("Active mechanic profile is required.");
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

    public record TimerActionRequest(Guid WorkOrderId);

    [HttpPost("timer/start")]
    public async Task<ActionResult<ApiResponse<object>>> StartTimer(
        [FromBody] TimerActionRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var wo = await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        if (wo.Status != WorkOrderStatus.Assigned &&
            wo.Status != WorkOrderStatus.InProgress &&
            wo.Status != WorkOrderStatus.WaitingForParts)
            throw new ConflictException(
                "This job cannot be started in its current status.");

        var active = await _db.LaborSessions.AnyAsync(s =>
            s.MechanicStaffId == mechanicId &&
            s.Status == LaborSessionStatus.Active);

        if (active)
            throw new ConflictException(
                "Mechanic already has an active timer.");

        var now = DateTime.UtcNow;

        var session = new LaborSession
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = mechanicId,
            StartedAt = now,
            Status = LaborSessionStatus.Active
        };

        wo.Status = WorkOrderStatus.InProgress;
        wo.StartedAt ??= now;
        wo.UpdatedAt = now;

        _db.LaborSessions.Add(session);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(session, "Timer started."));
    }

    [HttpPost("timer/end")]
    public async Task<ActionResult<ApiResponse<object>>> EndTimer(
        [FromBody] TimerActionRequest request)
    {
        var mechanicId = await CurrentMechanicId();

        await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        var session = await _db.LaborSessions
            .Where(s =>
                s.MechanicStaffId == mechanicId &&
                s.WorkOrderId == request.WorkOrderId &&
                (s.Status == LaborSessionStatus.Active ||
                 s.Status == LaborSessionStatus.Paused))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException(
                "No active labor session found.");

        session.EndedAt = DateTime.UtcNow;
        session.Status = LaborSessionStatus.Ended;

        var totalSeconds =
            (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds
            - session.PauseSeconds;

        session.DurationSeconds = Math.Max(0, totalSeconds);

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(session, "Timer ended."));
    }

    public record CreateDiagnosticRequest(
        Guid WorkOrderId,
        string Finding,
        string? Severity);

    public record CreateRepairRequest(
        Guid WorkOrderId,
        string Action,
        string? Notes);

    public record CreateRecommendationRequest(
        Guid WorkOrderId,
        string Recommendation,
        string? Priority);

    public record UpdateJobStatusRequest(
        WorkOrderStatus Status,
        string? Notes);

    [HttpPost("diagnostics")]
    public async Task<ActionResult<ApiResponse<object>>> AddDiagnostic(
        [FromBody] CreateDiagnosticRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder =
            await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        if (workOrder.Status is WorkOrderStatus.Completed
            or WorkOrderStatus.Cancelled)
            throw new ConflictException(
                "Diagnostics cannot be added to a completed or cancelled job.");

        if (string.IsNullOrWhiteSpace(request.Finding))
            throw new ValidationException("Finding is required.");

        var item = new DiagnosticFinding
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = mechanicId,
            Finding = request.Finding.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity)
                ? null
                : request.Severity.Trim()
        };

        _db.DiagnosticFindings.Add(item);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            item,
            "Diagnostic finding recorded."));
    }

    [HttpPost("repairs")]
    public async Task<ActionResult<ApiResponse<object>>> AddRepair(
        [FromBody] CreateRepairRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder =
            await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        if (workOrder.Status is WorkOrderStatus.Completed
            or WorkOrderStatus.Cancelled)
            throw new ConflictException(
                "Repair actions cannot be added to a completed or cancelled job.");

        if (string.IsNullOrWhiteSpace(request.Action))
            throw new ValidationException("Repair action is required.");

        var item = new RepairAction
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = mechanicId,
            Action = request.Action.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim()
        };

        _db.RepairActions.Add(item);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            item,
            "Repair action recorded."));
    }

    [HttpPost("recommendations")]
    public async Task<ActionResult<ApiResponse<object>>> AddRecommendation(
        [FromBody] CreateRecommendationRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder =
            await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        if (workOrder.Status is WorkOrderStatus.Completed
            or WorkOrderStatus.Cancelled)
            throw new ConflictException(
                "Recommendations cannot be added to a completed or cancelled job.");

        if (string.IsNullOrWhiteSpace(request.Recommendation))
            throw new ValidationException("Recommendation is required.");

        var item = new MechanicRecommendation
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = mechanicId,
            Recommendation = request.Recommendation.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority)
                ? null
                : request.Priority.Trim()
        };

        _db.MechanicRecommendations.Add(item);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            item,
            "Recommendation recorded."));
    }

    [HttpPatch("jobs/{workOrderId:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateJobStatus(
        Guid workOrderId,
        [FromBody] UpdateJobStatusRequest request)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder =
            await GetAssignedWorkOrder(mechanicId, workOrderId);

        if (workOrder.Status == request.Status)
            return Ok(ApiResponse<object>.Ok(
                workOrder,
                "Job status is already set to the requested value."));

        var allowed = (workOrder.Status, request.Status) switch
        {
            (WorkOrderStatus.Assigned, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.WaitingForParts) => true,
            (WorkOrderStatus.WaitingForParts, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
            _ => false
        };

        if (!allowed)
            throw new ConflictException(
                $"Cannot change job status from {workOrder.Status} to {request.Status}.");

        if (request.Status == WorkOrderStatus.Completed)
            return await CompleteJobInternal(
                mechanicId,
                workOrder,
                request.Notes);

        var now = DateTime.UtcNow;

        workOrder.Status = request.Status;

        if (request.Status == WorkOrderStatus.InProgress)
            workOrder.StartedAt ??= now;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            workOrder.TechnicianNotes = request.Notes.Trim();

        workOrder.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            workOrder,
            "Job status updated."));
    }

    [HttpPost("jobs/{workOrderId:guid}/complete")]
    public async Task<ActionResult<ApiResponse<object>>> CompleteJob(
        Guid workOrderId)
    {
        var mechanicId = await CurrentMechanicId();
        var workOrder =
            await GetAssignedWorkOrder(mechanicId, workOrderId);

        if (workOrder.Status != WorkOrderStatus.InProgress)
            throw new ConflictException(
                "Only an in-progress job can be completed.");

        return await CompleteJobInternal(
            mechanicId,
            workOrder,
            null);
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

    public record CreateRequisitionRequest(
        Guid WorkOrderId,
        Guid? PartId,
        string PartSpec,
        int QtyRequested,
        RequisitionUrgency Urgency,
        string? Reason);

    [HttpPost("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRequisition(
        [FromBody] CreateRequisitionRequest request)
    {
        var mechanicId = await CurrentMechanicId();

        await GetAssignedWorkOrder(mechanicId, request.WorkOrderId);

        if (request.QtyRequested <= 0)
            throw new ValidationException(
                "Quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.PartSpec))
            throw new ValidationException(
                "Part specification is required.");

        var req = new PartRequisition
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            RequestedByStaffId = mechanicId,
            PartId = request.PartId,
            PartSpec = request.PartSpec.Trim(),
            QtyRequested = request.QtyRequested,
            Urgency = request.Urgency,
            Reason = string.IsNullOrWhiteSpace(request.Reason)
                ? null
                : request.Reason.Trim(),
            Status = RequisitionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.PartRequisitions.Add(req);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            req,
            "Requisition submitted."));
    }
}
