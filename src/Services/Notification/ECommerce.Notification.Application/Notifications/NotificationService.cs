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
        NotificationRecord notification = new(
            request.CustomerId,
            request.OrderId,
            request.Type,
            request.Title,
            request.Message,
            request.Culture,
            NotificationChannel.Realtime);

        notificationRepository.Add(notification);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        NotificationMessage message = new(
            notification.Id,
            notification.CustomerId,
            notification.OrderId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Culture,
            notification.CreatedAt);

        await liveNotificationPublisher.PublishAsync(message, cancellationToken);
        return message;
    }
}
