namespace ECommerce.Notification.Application.Notifications;

public sealed record CreateNotificationRequest(
    Guid CustomerId,
    Guid? OrderId,
    string Type,
    string Title,
    string Message,
    string Culture);
