namespace ECommerce.Notification.Application.Notifications;

public interface INotificationCreationStore
{
    Task<NotificationCreationResult> CreateIfMissingAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken);
}
