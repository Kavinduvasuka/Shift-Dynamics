using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public WorkOrdersController(ShiftDynamicsDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAll(
        [FromQuery] WorkOrderStatus? status)
    {
        var query = _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.Service)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                w.Id,
                w.WorkOrderNumber,
                w.Status,
                w.CustomerId,
                CustomerName = w.Customer.FirstName + " " + w.Customer.LastName,
                w.VehicleId,
                VehicleReg = w.Vehicle.RegistrationNumber,
                w.ServiceId,
                ServiceName = w.Service.Name,
                w.AssignedStaffId,
                w.Description,
                w.StartedAt,
                w.CompletedAt,
                w.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var w = await _db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Include(x => x.Service)
            .Include(x => x.AssignedStaff)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Work order not found.");

        return Ok(ApiResponse<object>.Ok(w));
    }

    public record CreateWorkOrderRequest(
        Guid CustomerId,
        Guid VehicleId,
        Guid ServiceId,
        Guid? AppointmentId,
        string? Description);

    [HttpPost]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateWorkOrderRequest request)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists) throw new NotFoundException("Customer not found.");

        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId)
            ?? throw new NotFoundException("Vehicle not found.");

        if (vehicle.CustomerId != request.CustomerId)
            throw new ValidationException("Vehicle does not belong to the specified customer.");

        if (!await _db.Services.AnyAsync(s => s.Id == request.ServiceId && s.IsActive))
            throw new NotFoundException("Service not found or inactive.");

        var count = await _db.WorkOrders.CountAsync() + 1;
        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{count:D4}",
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            ServiceId = request.ServiceId,
            AppointmentId = request.AppointmentId,
            Description = request.Description,
            Status = WorkOrderStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workOrder.Id },
            ApiResponse<object>.Ok(workOrder, "Work order created."));
    }

    public record UpdateStatusRequest(WorkOrderStatus Status, string? Notes);

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Staff")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var wo = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new NotFoundException("Work order not found.");

        wo.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            wo.TechnicianNotes = request.Notes;
        if (request.Status == WorkOrderStatus.InProgress && wo.StartedAt is null)
            wo.StartedAt = DateTime.UtcNow;
        if (request.Status == WorkOrderStatus.Completed)
            wo.CompletedAt = DateTime.UtcNow;
        wo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(wo, "Status updated."));
    }
}
