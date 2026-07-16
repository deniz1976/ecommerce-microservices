namespace ECommerce.Notification.Application.Notifications;

public interface ILiveNotificationPublisher
{
    Task PublishAsync(NotificationMessage notification, CancellationToken cancellationToken);
}
