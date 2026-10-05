using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Policy = "Manager")]
public class ManagerController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly IManagerService _manager;

    public ManagerController(ShiftDynamicsDbContext db, IManagerService manager) { _db = db; _manager = manager; }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> Dashboard()
    {
        return Ok(ApiResponse<object>.Ok(await _manager.GetDashboardAsync()));
    }

    [HttpGet("bays")]
    public async Task<ActionResult<ApiResponse<object>>> Bays()
    {
        return Ok(ApiResponse<object>.Ok(await _manager.GetBaysAsync()));
    }

    [HttpGet("workshop")]
    public async Task<ActionResult<ApiResponse<object>>> Workshop()
    {
        var bays = await _db.WorkshopBays
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();

        var assignments = await _db.JobAssignments
            .AsNoTracking()
            .Where(a => a.IsActive && a.BayId.HasValue)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w.Customer)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w.Service)
            .Include(a => a.Mechanic)
                .ThenInclude(s => s.User)
            .ToListAsync();

        var assignmentByBay = assignments
            .Where(a => a.BayId.HasValue)
            .ToDictionary(a => a.BayId!.Value);

        var data = bays.Select(b =>
        {
            assignmentByBay.TryGetValue(b.Id, out var a);

            return new
            {
                bayId = b.Id,
                bayName = b.Name,
                status = b.Status,
                notes = b.Notes,
                job = a == null ? null : new
                {
                    workOrderId = a.WorkOrderId,
                    workOrderNumber = a.WorkOrder.WorkOrderNumber,
                    status = a.WorkOrder.Status,
                    customerName = $"{a.WorkOrder.Customer.FirstName} {a.WorkOrder.Customer.LastName}".Trim(),
                    vehicle = $"{a.WorkOrder.Vehicle.Make} {a.WorkOrder.Vehicle.Model}".Trim(),
                    registrationNumber = a.WorkOrder.Vehicle.RegistrationNumber,
                    serviceName = a.WorkOrder.Service.Name,
                    mechanicStaffId = a.MechanicStaffId,
                    mechanicName = a.Mechanic.User.FullName,
                    assignedAt = a.AssignedAt,
                    startedAt = a.WorkOrder.StartedAt,
                    completedAt = a.WorkOrder.CompletedAt
                }
            };
        }).ToList();

        return Ok(ApiResponse<object>.Ok(data));
    }
    [HttpGet("job-cards")]
    public async Task<ActionResult<ApiResponse<object>>> JobCards()
    {
        var jobs = await _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.Service)
            .Include(w => w.AssignedStaff)
                .ThenInclude(s => s!.User)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                workOrderId = w.Id,
                workOrderNumber = w.WorkOrderNumber,
                status = w.Status,
                customerName = $"{w.Customer.FirstName} {w.Customer.LastName}".Trim(),
                vehicle = $"{w.Vehicle.Make} {w.Vehicle.Model}".Trim(),
                registrationNumber = w.Vehicle.RegistrationNumber,
                serviceName = w.Service.Name,
                mechanicStaffId = w.AssignedStaffId,
                mechanicName = w.AssignedStaff != null
                    ? w.AssignedStaff.User.FullName
                    : null,
                startedAt = w.StartedAt,
                completedAt = w.CompletedAt,
                createdAt = w.CreatedAt,
                updatedAt = w.UpdatedAt
            })
            .ToListAsync();

        var activeAssignments = await _db.JobAssignments
            .AsNoTracking()
            .Where(a => a.IsActive && a.BayId.HasValue)
            .Select(a => new
            {
                a.WorkOrderId,
                bayId = a.BayId,
                bayName = a.Bay != null ? a.Bay.Name : null,
                assignedAt = a.AssignedAt
            })
            .ToListAsync();

        var assignmentByJob = activeAssignments
            .GroupBy(a => a.WorkOrderId)
            .ToDictionary(g => g.Key, g => g.First());

        var data = jobs.Select(job =>
        {
            assignmentByJob.TryGetValue(job.workOrderId, out var assignment);

            return new
            {
                job.workOrderId,
                job.workOrderNumber,
                job.status,
                job.customerName,
                job.vehicle,
                job.registrationNumber,
                job.serviceName,
                job.mechanicStaffId,
                job.mechanicName,
                bayId = assignment?.bayId,
                bayName = assignment?.bayName,
                assignedAt = assignment?.assignedAt,
                job.startedAt,
                job.completedAt,
                job.createdAt,
                job.updatedAt
            };
        }).ToList();

        return Ok(ApiResponse<object>.Ok(data));
    }
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<object>>> Summary()
    {
        var activeJobCards = await _db.WorkOrders.CountAsync(w =>
            w.Status == WorkOrderStatus.Open ||
            w.Status == WorkOrderStatus.Assigned ||
            w.Status == WorkOrderStatus.InProgress);

        var mechanicsOnDuty = await _db.Staff.CountAsync(s =>
            s.Role == SystemRole.Mechanic &&
            s.Status == StaffStatus.Active);

        var pendingVendorApprovals = await _db.VendorRegistrations.CountAsync(v =>
            v.Status == VendorRegistrationStatus.Pending);

        var pendingInvoiceApprovals = await _db.Invoices.CountAsync(i =>
            i.Status == InvoiceStatus.Draft);

        var pendingApprovals = pendingVendorApprovals + pendingInvoiceApprovals;

        var vendorQuotes = 0;

        return Ok(ApiResponse<object>.Ok(new
        {
            activeJobCards,
            mechanicsOnDuty,
            vendorQuotes,
            pendingApprovals,
            pendingVendorApprovals,
            pendingInvoiceApprovals
        }));
    }
    public record UpsertBayRequest(string Name, BayStatus Status, string? Notes);

    [HttpPost("bays")]
    public async Task<ActionResult<ApiResponse<object>>> CreateBay([FromBody] UpsertBayRequest request)
    {
        var bay = await _manager.CreateBayAsync(request.Name, request.Status, request.Notes);
        return Ok(ApiResponse<object>.Ok(bay));
    }

    public record AssignJobRequest(Guid WorkOrderId, Guid MechanicStaffId, Guid? BayId);

    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponse<object>>> Assign([FromBody] AssignJobRequest request)
    {
        var assignment = await _manager.AssignJobAsync(request.WorkOrderId, request.MechanicStaffId, request.BayId, User.RequireUserId());

        return Ok(ApiResponse<object>.Ok(assignment, "Job assigned."));
    }

    [HttpGet("mechanics")]
    public async Task<ActionResult<ApiResponse<object>>> Mechanics()
    {
        return Ok(ApiResponse<object>.Ok(await _manager.GetMechanicsAsync()));
    }
}

