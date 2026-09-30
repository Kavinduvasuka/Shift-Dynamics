using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;

namespace ShiftDynamics.API.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid userId, string type, string title, string body, string? entityType = null, Guid? entityId = null, CancellationToken ct = default);
}

public class NotificationService : INotificationService
{
    private readonly ShiftDynamicsDbContext _db;

    public NotificationService(ShiftDynamicsDbContext db) => _db = db;

    public async Task NotifyAsync(Guid userId, string type, string title, string body, string? entityType = null, Guid? entityId = null, CancellationToken ct = default)
    {
        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            EntityType = entityType,
            EntityId = entityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
