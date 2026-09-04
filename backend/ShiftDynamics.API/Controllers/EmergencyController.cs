using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using System.Security.Claims;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/emergency")]
public class EmergencyController : ControllerBase
{
    private readonly ShiftDynamicsDbContext _db;

    public EmergencyController(ShiftDynamicsDbContext db) => _db = db;

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

        // Simple haversine filter when coordinates provided
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
        public Guid? CustomerId { get; set; }
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
            throw new ShiftDynamics.API.Common.ValidationException("Vehicle does not belong to the authenticated customer.");

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

        // If no customer, we still store the request (CustomerId may be empty Guid for public)
        _db.EmergencyRequests.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Status }, "Emergency request submitted."));
    }

    [HttpGet("requests")]
    [Authorize(Policy = "Staff")]
    public async Task<ActionResult<ApiResponse<object>>> ListRequests([FromQuery] EmergencyRequestStatus? status)
    {
        var query = _db.EmergencyRequests.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        var items = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
        return Ok(ApiResponse<object>.Ok(items));
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
