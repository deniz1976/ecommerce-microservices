using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public interface INotificationReader
{
    Task<NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        NotificationChannel channel,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificationRecord>> GetUnreadByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken);
}
