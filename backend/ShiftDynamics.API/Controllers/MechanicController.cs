using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using System.Security.Claims;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/mechanic")]
[Authorize(Policy = "Mechanic")]
public class MechanicController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public MechanicController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet("jobs")]
    public async Task<ActionResult<ApiResponse<object>>> MyJobs()
    {
        // Resolve staff from user – simplified: return all assigned active jobs
        var jobs = await _db.JobAssignments
            .AsNoTracking()
            .Include(a => a.WorkOrder).ThenInclude(w => w.Vehicle)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Customer)
            .Include(a => a.WorkOrder).ThenInclude(w => w.Service)
            .Where(a => a.IsActive)
            .Select(a => new
            {
                a.Id,
                a.WorkOrderId,
                a.WorkOrder.WorkOrderNumber,
                a.WorkOrder.Status,
                Vehicle = a.WorkOrder.Vehicle.RegistrationNumber,
                Customer = a.WorkOrder.Customer.FirstName + " " + a.WorkOrder.Customer.LastName,
                Service = a.WorkOrder.Service.Name,
                a.BayId,
                a.AssignedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(jobs));
    }

    public record TimerActionRequest(Guid WorkOrderId, Guid MechanicStaffId);

    [HttpPost("timer/start")]
    public async Task<ActionResult<ApiResponse<object>>> StartTimer([FromBody] TimerActionRequest request)
    {
        var active = await _db.LaborSessions.AnyAsync(s =>
            s.MechanicStaffId == request.MechanicStaffId &&
            s.Status == LaborSessionStatus.Active);

        if (active)
            throw new ConflictException("Mechanic already has an active timer.");

        var session = new LaborSession
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            MechanicStaffId = request.MechanicStaffId,
            StartedAt = DateTime.UtcNow,
            Status = LaborSessionStatus.Active
        };

        var wo = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId);
        if (wo is not null)
        {
            wo.Status = WorkOrderStatus.InProgress;
            wo.StartedAt ??= DateTime.UtcNow;
            wo.UpdatedAt = DateTime.UtcNow;
        }

        _db.LaborSessions.Add(session);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(session, "Timer started."));
    }

    [HttpPost("timer/end")]
    public async Task<ActionResult<ApiResponse<object>>> EndTimer([FromBody] TimerActionRequest request)
    {
        var session = await _db.LaborSessions
            .Where(s => s.MechanicStaffId == request.MechanicStaffId &&
                        s.WorkOrderId == request.WorkOrderId &&
                        (s.Status == LaborSessionStatus.Active || s.Status == LaborSessionStatus.Paused))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("No active labor session found.");

        session.EndedAt = DateTime.UtcNow;
        session.Status = LaborSessionStatus.Ended;

        var totalSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds - session.PauseSeconds;
        session.DurationSeconds = Math.Max(0, totalSeconds);

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(session, "Timer ended."));
    }

    public record CreateRequisitionRequest(
        Guid WorkOrderId, Guid RequestedByStaffId, Guid? PartId,
        string PartSpec, int QtyRequested, RequisitionUrgency Urgency, string? Reason);

    [HttpPost("requisitions")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRequisition([FromBody] CreateRequisitionRequest request)
    {
        if (request.QtyRequested <= 0)
            throw new ValidationException("Quantity must be greater than zero.");

        var req = new PartRequisition
        {
            Id = Guid.NewGuid(),
            WorkOrderId = request.WorkOrderId,
            RequestedByStaffId = request.RequestedByStaffId,
            PartId = request.PartId,
            PartSpec = request.PartSpec.Trim(),
            QtyRequested = request.QtyRequested,
            Urgency = request.Urgency,
            Reason = request.Reason,
            Status = RequisitionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.PartRequisitions.Add(req);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(req, "Requisition submitted."));
    }
}
