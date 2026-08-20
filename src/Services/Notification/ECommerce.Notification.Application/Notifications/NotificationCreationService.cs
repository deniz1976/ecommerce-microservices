namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationCreationService(
    INotificationCreationStore creationStore,
    ILiveNotificationPublisher liveNotificationPublisher)
{
    public async Task<NotificationMessage> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationCreationResult result = await creationStore.CreateIfMissingAsync(
            request,
            cancellationToken);
        NotificationMessage message = NotificationMapper.ToMessage(result.Notification);
        if (result.WasCreated)
        {
            await liveNotificationPublisher.PublishAsync(message, cancellationToken);
        }

        return message;
    }
}
