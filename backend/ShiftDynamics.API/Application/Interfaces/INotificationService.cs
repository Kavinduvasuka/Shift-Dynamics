using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Application.Interfaces;

using ShiftDynamics.API.Application.DTOs.Notifications;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string type, string title, string body, string? entityType = null, Guid? entityId = null);
    Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(Guid userId, bool unreadOnly);
    Task<bool> MarkReadAsync(Guid id, Guid userId);
}

