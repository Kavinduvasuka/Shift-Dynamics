using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class HealthService : IHealthService
{
    private readonly ShiftDynamicsDbContext _db;
    public HealthService(ShiftDynamicsDbContext db) => _db = db;
    public object GetLiveness(string environment) => new { status = "healthy", service = "Shift Dynamics API", environment, timestamp = DateTime.UtcNow };
    public async Task<object> GetReadinessAsync(CancellationToken cancellationToken = default) => new { status = await _db.Database.CanConnectAsync(cancellationToken) ? "ready" : "not_ready", database = await _db.Database.CanConnectAsync(cancellationToken), timestamp = DateTime.UtcNow };
}

