namespace ECommerce.Notification.Application.Notifications;

public sealed record CreateNotificationRequest(
    Guid SourceMessageId,
    Guid CustomerId,
    Guid? OrderId,
    string Type,
    string Title,
    string Message,
    string Culture,
    string? ReasonCode = null,
    string? TrackingNumber = null);
