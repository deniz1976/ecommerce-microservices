using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationCreationService
{
    private readonly IRepository<NotificationRecord, Guid> notificationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly INotificationReader notificationReader;
    private readonly ILiveNotificationPublisher liveNotificationPublisher;

    public NotificationCreationService(
        IRepository<NotificationRecord, Guid> notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationReader notificationReader,
        ILiveNotificationPublisher liveNotificationPublisher)
    {
        this.notificationRepository = notificationRepository;
        this.unitOfWork = unitOfWork;
        this.notificationReader = notificationReader;
        this.liveNotificationPublisher = liveNotificationPublisher;
    }

    public async Task<NotificationMessage> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationRecord? existingNotification = await notificationReader.FindBySourceAsync(
            request.SourceMessageId,
            NotificationChannel.Realtime,
            cancellationToken);
        if (existingNotification is not null)
        {
            return NotificationMapper.ToMessage(existingNotification);
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        NotificationMessage message = NotificationMapper.ToMessage(notification);
        await liveNotificationPublisher.PublishAsync(message, cancellationToken);
        return message;
    }
}
