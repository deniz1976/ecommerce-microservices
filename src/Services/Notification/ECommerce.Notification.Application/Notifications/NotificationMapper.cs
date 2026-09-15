using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public static class NotificationMapper
{
    public static NotificationMessage ToMessage(NotificationRecord notification)
    {
        return new NotificationMessage(
            notification.Id,
            notification.CustomerId,
            notification.OrderId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Culture,
            notification.ReasonCode,
            notification.TrackingNumber,
            notification.CreatedAt,
            notification.ReadAt);
    }
}
