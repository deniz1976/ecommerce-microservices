using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public interface INotificationReader
{
    Task<NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        NotificationChannel channel,
        CancellationToken cancellationToken);
}
