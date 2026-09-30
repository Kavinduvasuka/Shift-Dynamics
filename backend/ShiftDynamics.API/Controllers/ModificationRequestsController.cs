using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Services;
using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/modification-requests")]
[Authorize]
public class ModificationRequestsController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;

    public ModificationRequestsController(ShiftDynamicsDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public class CreateModificationRequest
    {
        [Required] public Guid VehicleId { get; set; }
        [Required, StringLength(2000)] public string Request { get; set; } = string.Empty;
        [StringLength(1000)] public string? Notes { get; set; }
    }

    public class ReviewModificationRequest
    {
        [Required] public bool Approve { get; set; }
        [StringLength(1000)] public string? ReviewNotes { get; set; }
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateModificationRequest request)
    {
        var customerId = User.RequireCustomerId();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.CustomerId == customerId)
            ?? throw new NotFoundException("Vehicle not found or does not belong to you.");

        var entity = new ModificationRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicle.Id,
            Request = request.Request.Trim(),
            Notes = request.Notes?.Trim(),
            Status = ModificationRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.ModificationRequests.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            entity.Id,
            entity.VehicleId,
            entity.Request,
            entity.Notes,
            entity.Status,
            entity.CreatedAt
        }, "Modification request submitted."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] ModificationRequestStatus? status)
    {
        var role = User.GetRole();
        IQueryable<ModificationRequest> query = _db.ModificationRequests.AsNoTracking()
            .Include(m => m.Vehicle)
            .Include(m => m.Customer);

        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            query = query.Where(m => m.CustomerId == customerId);
        }
        else if (role is "Manager" or "Admin" or "ServiceAdvisor")
        {
        }
        else
        {
            throw new ForbiddenException();
        }

        if (status.HasValue) query = query.Where(m => m.Status == status.Value);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.CustomerId,
                CustomerName = m.Customer.FirstName + " " + m.Customer.LastName,
                m.VehicleId,
                Vehicle = new { m.Vehicle.Make, m.Vehicle.Model, m.Vehicle.RegistrationNumber },
                m.Request,
                m.Notes,
                m.Status,
                m.ReviewedByUserId,
                m.ReviewNotes,
                m.CreatedAt,
                m.UpdatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Get(Guid id)
    {
        var m = await _db.ModificationRequests.AsNoTracking()
            .Include(x => x.Vehicle)
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modification request not found.");

        var role = User.GetRole();
        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            if (m.CustomerId != customerId) throw new ForbiddenException();
        }
        else if (role is not ("Manager" or "Admin" or "ServiceAdvisor"))
        {
            throw new ForbiddenException();
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            m.Id,
            m.CustomerId,
            CustomerName = m.Customer.FirstName + " " + m.Customer.LastName,
            m.VehicleId,
            Vehicle = new { m.Vehicle.Make, m.Vehicle.Model, m.Vehicle.RegistrationNumber },
            m.Request,
            m.Notes,
            m.Status,
            m.ReviewedByUserId,
            m.ReviewNotes,
            m.CreatedAt,
            m.UpdatedAt
        }));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id)
    {
        var customerId = User.RequireCustomerId();
        var m = await _db.ModificationRequests.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId)
            ?? throw new NotFoundException("Modification request not found.");

        if (m.Status != ModificationRequestStatus.Pending)
            throw new ConflictException("Only pending requests can be cancelled.");

        m.Status = ModificationRequestStatus.Cancelled;
        m.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { m.Id, m.Status }, "Request cancelled."));
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Review(Guid id, [FromBody] ReviewModificationRequest request)
    {
        var userId = User.RequireUserId();
        var m = await _db.ModificationRequests.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modification request not found.");

        if (m.Status != ModificationRequestStatus.Pending)
            throw new ConflictException("Only pending requests can be reviewed.");

        m.Status = request.Approve ? ModificationRequestStatus.Approved : ModificationRequestStatus.Rejected;
        m.ReviewedByUserId = userId;
        m.ReviewNotes = request.ReviewNotes?.Trim();
        m.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var customerUser = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == m.CustomerId);
        if (customerUser != null)
        {
            await _notifications.NotifyAsync(
                customerUser.Id,
                "modification_review",
                request.Approve ? "Modification request approved" : "Modification request rejected",
                $"Your vehicle modification request has been {(request.Approve ? "approved" : "rejected")}.",
                "ModificationRequest",
                m.Id);
        }

        return Ok(ApiResponse<object>.Ok(new { m.Id, m.Status, m.ReviewNotes }, "Review recorded."));
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] StatusUpdateRequest request)
    {
        var m = await _db.ModificationRequests.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modification request not found.");

        var allowed = m.Status switch
        {
            ModificationRequestStatus.Approved => request.Status is ModificationRequestStatus.InProgress or ModificationRequestStatus.Cancelled,
            ModificationRequestStatus.InProgress => request.Status is ModificationRequestStatus.Completed or ModificationRequestStatus.Cancelled,
            _ => false
        };

        if (!allowed)
            throw new ConflictException($"Cannot transition from {m.Status} to {request.Status}.");

        m.Status = request.Status;
        m.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var customerUser = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == m.CustomerId);
        if (customerUser != null)
        {
            await _notifications.NotifyAsync(
                customerUser.Id,
                "modification_status",
                "Modification request update",
                $"Your modification request status is now {m.Status}.",
                "ModificationRequest",
                m.Id);
        }

        return Ok(ApiResponse<object>.Ok(new { m.Id, m.Status }, "Status updated."));
    }

    public record StatusUpdateRequest(ModificationRequestStatus Status);
}
