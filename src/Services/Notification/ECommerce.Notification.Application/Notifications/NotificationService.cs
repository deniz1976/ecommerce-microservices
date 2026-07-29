using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationService
{
    private readonly IRepository<NotificationRecord, Guid> notificationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly INotificationReader notificationReader;
    private readonly ILiveNotificationPublisher liveNotificationPublisher;
    private readonly TimeProvider timeProvider;

    public NotificationService(
        IRepository<NotificationRecord, Guid> notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationReader notificationReader,
        ILiveNotificationPublisher liveNotificationPublisher,
        TimeProvider timeProvider)
    {
        this.notificationRepository = notificationRepository;
        this.unitOfWork = unitOfWork;
        this.notificationReader = notificationReader;
        this.liveNotificationPublisher = liveNotificationPublisher;
        this.timeProvider = timeProvider;
    }

    public async Task<NotificationMessage> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
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

    public async Task<Result<NotificationMessage>> MarkReadAsync(
        Guid customerId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        NotificationRecord? notification =
            await notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null || notification.CustomerId != customerId)
        {
            return Result<NotificationMessage>.Failure(
                new Error(
                    ErrorCodes.NotificationNotFound,
                    ErrorCodes.NotificationNotFound));
        }

        if (notification.MarkRead(timeProvider.GetUtcNow()))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<NotificationMessage>.Success(
            NotificationMapper.ToMessage(notification));
    }
}
