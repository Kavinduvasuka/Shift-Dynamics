using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class DatabaseService : IDatabaseService
{
    private readonly ShiftDynamicsDbContext _db;
    public DatabaseService(ShiftDynamicsDbContext db) => _db = db;
    public async Task<object> GetHealthAsync(CancellationToken cancellationToken = default) => new { database = "shift_dynamics", connected = await _db.Database.CanConnectAsync(cancellationToken), timestamp = DateTime.UtcNow };
}

