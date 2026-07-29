namespace ECommerce.Notification.Application.Notifications;

public sealed record NotificationMessage(
    Guid Id,
    Guid CustomerId,
    Guid? OrderId,
    string Type,
    string Title,
    string Message,
    string Culture,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
