namespace ECommerce.Notification.Application.Notifications;

public interface INotificationRepository
{
    Task<Domain.NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        Domain.NotificationChannel channel,
        CancellationToken cancellationToken);

    void Add(Domain.NotificationRecord notification);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
