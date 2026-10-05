using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class EmergencyService : IEmergencyService
{
    private readonly ShiftDynamicsDbContext _db;
    public EmergencyService(ShiftDynamicsDbContext db) => _db = db;

    public async Task<IReadOnlyList<EmergencyServiceProvider>> FindProvidersAsync(decimal? latitude, decimal? longitude, string? category, double radiusKm)
    {
        if (radiusKm <= 0 || radiusKm > 500) throw new ValidationException("Radius must be between 0 and 500 km.");
        if (latitude.HasValue != longitude.HasValue) throw new ValidationException("Latitude and longitude must be supplied together.");
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180) throw new ValidationException("Coordinates are outside valid ranges.");
        var query = _db.EmergencyServiceProviders.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(p => p.Category == category.Trim());
        var providers = await query.ToListAsync();
        if (!latitude.HasValue) return providers.OrderBy(p => p.Name).ToList();
        return providers.Select(p => new { Provider = p, Distance = HaversineKm((double)latitude.Value, (double)longitude!.Value, (double)p.Latitude, (double)p.Longitude) }).Where(x => x.Distance <= radiusKm).OrderBy(x => x.Distance).Select(x => x.Provider).ToList();
    }

    public async Task<EmergencyRequest> CreateRequestAsync(Guid customerId, Guid? vehicleId, string location, decimal? latitude, decimal? longitude, string problemDescription)
    {
        if (latitude.HasValue != longitude.HasValue || latitude is < -90 or > 90 || longitude is < -180 or > 180) throw new ValidationException("Coordinates are invalid.");
        if (vehicleId.HasValue && !await _db.Vehicles.AnyAsync(v => v.Id == vehicleId.Value && v.CustomerId == customerId)) throw new ValidationException("Vehicle does not belong to the authenticated customer.");
        var request = new EmergencyRequest { Id = Guid.NewGuid(), CustomerId = customerId, VehicleId = vehicleId, Location = location.Trim(), Latitude = latitude, Longitude = longitude, ProblemDescription = problemDescription.Trim(), Status = EmergencyRequestStatus.Pending, RequestedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        _db.EmergencyRequests.Add(request); await _db.SaveChangesAsync(); return request;
    }

    public async Task<IReadOnlyList<EmergencyRequest>> ListRequestsAsync(EmergencyRequestStatus? status)
    {
        var query = _db.EmergencyRequests.AsNoTracking().AsQueryable(); if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        return await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371; var deltaLat = (lat2 - lat1) * Math.PI / 180; var deltaLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}

