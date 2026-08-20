using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed record NotificationDelivery(
    Guid DispatchId,
    Guid CustomerId,
    Guid? OrderId,
    string Type,
    string Title,
    string Message,
    string Culture,
    NotificationDispatchChannel Channel);
