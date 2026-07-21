using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly ILiveNotificationPublisher liveNotificationPublisher;

    public NotificationService(INotificationRepository notificationRepository, ILiveNotificationPublisher liveNotificationPublisher)
    {
        this.notificationRepository = notificationRepository;
        this.liveNotificationPublisher = liveNotificationPublisher;
    }

    public async Task<NotificationMessage> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        NotificationRecord? existingNotification = await notificationRepository.FindBySourceAsync(
            request.SourceMessageId,
            NotificationChannel.Realtime,
            cancellationToken);

        if (existingNotification is not null)
        {
            return ToMessage(existingNotification);
        }

        NotificationRecord notification = new(
            request.SourceMessageId,
            request.CustomerId,
            request.OrderId,
            request.Type,
            request.Title,
            request.Message,
            request.Culture,
            NotificationChannel.Realtime);

        notificationRepository.Add(notification);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        NotificationMessage message = ToMessage(notification);

        await liveNotificationPublisher.PublishAsync(message, cancellationToken);
        return message;
    }

    private static NotificationMessage ToMessage(NotificationRecord notification)
    {
        return new NotificationMessage(
            notification.Id,
            notification.CustomerId,
            notification.OrderId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Culture,
            notification.CreatedAt);
    }
}
