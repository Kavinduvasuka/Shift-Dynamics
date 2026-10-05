using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string type, string title, string body, string? entityType = null, Guid? entityId = null);
    Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, bool unreadOnly);
    Task<bool> MarkReadAsync(Guid id, Guid userId);
}
