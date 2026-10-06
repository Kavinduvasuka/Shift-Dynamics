namespace ShiftDynamics.API.Application.DTOs.Notifications;

public record NotificationResponse(Guid Id, string Type, string Title, string Body, string? EntityType, Guid? EntityId, bool IsRead, DateTime CreatedAt);
