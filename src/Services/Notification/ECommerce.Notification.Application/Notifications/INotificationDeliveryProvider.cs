using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public interface INotificationDeliveryProvider
{
    NotificationDispatchChannel Channel { get; }

    Task<NotificationDeliveryResult> DeliverAsync(
        NotificationDelivery delivery,
        CancellationToken cancellationToken);
}
