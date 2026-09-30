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
[Route("api/emergency")]
public class EmergencyController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;
    private readonly INotificationService _notifications;

    public EmergencyController(ShiftDynamicsDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    [HttpGet("services")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> NearbyServices(
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lng,
        [FromQuery] string? category,
        [FromQuery] double radiusKm = 25)
    {
        var query = _db.EmergencyServiceProviders.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        var providers = await query.ToListAsync();

        if (lat.HasValue && lng.HasValue)
        {
            providers = providers
                .Select(p => new
                {
                    Provider = p,
                    Distance = HaversineKm((double)lat.Value, (double)lng.Value, (double)p.Latitude, (double)p.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Provider)
                .ToList();
        }

        return Ok(ApiResponse<object>.Ok(providers));
    }

    public class CreateEmergencyRequest
    {
        public Guid? VehicleId { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [Required] public string ProblemDescription { get; set; } = string.Empty;
        [Required] public string ContactName { get; set; } = string.Empty;
        [Required] public string ContactPhone { get; set; } = string.Empty;
    }

    [HttpPost("requests")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRequest([FromBody] CreateEmergencyRequest request)
    {
        var customerId = User.RequireCustomerId();
        if (request.VehicleId.HasValue && !await _db.Vehicles.AnyAsync(v => v.Id == request.VehicleId && v.CustomerId == customerId))
            throw new Common.ValidationException("Vehicle does not belong to the authenticated customer.");

        var entity = new EmergencyRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = request.VehicleId,
            Location = request.Location.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ProblemDescription = request.ProblemDescription.Trim(),
            Status = EmergencyRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmergencyRequests.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Status }, "Emergency request submitted."));
    }

    [HttpGet("requests")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ListRequests([FromQuery] EmergencyRequestStatus? status)
    {
        var role = User.GetRole();
        IQueryable<EmergencyRequest> query = _db.EmergencyRequests.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle);

        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            query = query.Where(r => r.CustomerId == customerId);
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin" or "Mechanic"))
        {
            throw new ForbiddenException();
        }

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var items = await query.OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                r.Id,
                r.CustomerId,
                CustomerName = r.Customer.FirstName + " " + r.Customer.LastName,
                r.VehicleId,
                Vehicle = r.Vehicle != null ? r.Vehicle.Make + " " + r.Vehicle.Model + " (" + r.Vehicle.RegistrationNumber + ")" : null,
                r.Location,
                r.Latitude,
                r.Longitude,
                r.ProblemDescription,
                r.Status,
                r.AssignedStaffId,
                r.RequestedAt,
                r.AcceptedAt,
                r.CompletedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("requests/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> GetRequest(Guid id)
    {
        var r = await _db.EmergencyRequests.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Emergency request not found.");

        var role = User.GetRole();
        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            if (r.CustomerId != customerId) throw new ForbiddenException();
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin" or "Mechanic"))
        {
            throw new ForbiddenException();
        }

        return Ok(ApiResponse<object>.Ok(r));
    }

    public record AssignEmergencyRequest(Guid StaffId);

    [HttpPost("requests/{id:guid}/assign")]
    [Authorize(Policy = "ServiceAdvisor")]
    public async Task<ActionResult<ApiResponse<object>>> Assign(Guid id, [FromBody] AssignEmergencyRequest request)
    {
        var entity = await _db.EmergencyRequests.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Emergency request not found.");

        if (entity.Status is EmergencyRequestStatus.Completed or EmergencyRequestStatus.Cancelled)
            throw new ConflictException("Cannot assign a completed or cancelled request.");

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.Status == StaffStatus.Active)
            ?? throw new NotFoundException("Staff member not found or inactive.");

        entity.AssignedStaffId = staff.Id;
        await _db.SaveChangesAsync();

        var customerUser = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == entity.CustomerId);
        if (customerUser != null)
        {
            await _notifications.NotifyAsync(customerUser.Id, "emergency_assigned", "Emergency assistance assigned",
                "A staff member has been assigned to your emergency request.", "EmergencyRequest", entity.Id);
        }

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.AssignedStaffId, entity.Status }, "Staff assigned."));
    }

    public record UpdateEmergencyStatusRequest(EmergencyRequestStatus Status);

    [HttpPost("requests/{id:guid}/status")]
    [Authorize(Policy = "Staff")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] UpdateEmergencyStatusRequest request)
    {
        var entity = await _db.EmergencyRequests.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Emergency request not found.");

        var allowed = entity.Status switch
        {
            EmergencyRequestStatus.Pending => request.Status is EmergencyRequestStatus.Accepted or EmergencyRequestStatus.Cancelled,
            EmergencyRequestStatus.Accepted => request.Status is EmergencyRequestStatus.InProgress or EmergencyRequestStatus.Cancelled,
            EmergencyRequestStatus.InProgress => request.Status is EmergencyRequestStatus.Completed or EmergencyRequestStatus.Cancelled,
            _ => false
        };

        if (!allowed)
            throw new ConflictException($"Cannot transition emergency request from {entity.Status} to {request.Status}.");

        entity.Status = request.Status;
        if (request.Status == EmergencyRequestStatus.Accepted) entity.AcceptedAt = DateTime.UtcNow;
        if (request.Status == EmergencyRequestStatus.Completed) entity.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var customerUser = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == entity.CustomerId);
        if (customerUser != null)
        {
            await _notifications.NotifyAsync(customerUser.Id, "emergency_status", "Emergency request update",
                $"Your emergency request is now {entity.Status}.", "EmergencyRequest", entity.Id);
        }

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Status }, "Status updated."));
    }

    [HttpPost("requests/{id:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id)
    {
        var entity = await _db.EmergencyRequests.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Emergency request not found.");

        var role = User.GetRole();
        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            if (entity.CustomerId != customerId) throw new ForbiddenException();
            if (entity.Status is not (EmergencyRequestStatus.Pending or EmergencyRequestStatus.Accepted))
                throw new ConflictException("Only pending or accepted requests can be cancelled by the customer.");
        }
        else if (role is not ("ServiceAdvisor" or "Manager" or "Admin"))
        {
            throw new ForbiddenException();
        }

        if (entity.Status is EmergencyRequestStatus.Completed or EmergencyRequestStatus.Cancelled)
            throw new ConflictException("Request is already completed or cancelled.");

        entity.Status = EmergencyRequestStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Status }, "Emergency request cancelled."));
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
