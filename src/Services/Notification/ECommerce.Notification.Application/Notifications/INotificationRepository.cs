namespace ECommerce.Notification.Application.Notifications;

public interface INotificationRepository
{
    void Add(Domain.NotificationRecord notification);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
