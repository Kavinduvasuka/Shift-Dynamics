using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;
using ShiftDynamics.API.Infrastructure.Data;
using ShiftDynamics.API.Application.Interfaces;

namespace ShiftDynamics.API.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ShiftDynamicsDbContext _db;
    public NotificationService(ShiftDynamicsDbContext db) => _db = db;

    public async Task CreateAsync(Guid userId, string type, string title, string body, string? entityType = null, Guid? entityId = null)
    {
        _db.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = userId, Type = type, Title = title, Body = body, EntityType = entityType, EntityId = entityId, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, bool unreadOnly) =>
        await _db.Notifications.AsNoTracking().Where(n => n.UserId == userId && (!unreadOnly || !n.IsRead)).OrderByDescending(n => n.CreatedAt).ToListAsync();

    public async Task<bool> MarkReadAsync(Guid id, Guid userId)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification is null) return false;
        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }
}

