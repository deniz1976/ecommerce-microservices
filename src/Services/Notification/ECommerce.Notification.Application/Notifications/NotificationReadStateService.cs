using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationReadStateService
{
    private readonly IRepository<NotificationRecord, Guid> notificationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly INotificationReader notificationReader;
    private readonly TimeProvider timeProvider;

    public NotificationReadStateService(
        IRepository<NotificationRecord, Guid> notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationReader notificationReader,
        TimeProvider timeProvider)
    {
        this.notificationRepository = notificationRepository;
        this.unitOfWork = unitOfWork;
        this.notificationReader = notificationReader;
        this.timeProvider = timeProvider;
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

    public async Task<int> MarkAllReadAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<NotificationRecord> notifications =
            await notificationReader.GetUnreadByCustomerAsync(
                customerId,
                cancellationToken);
        DateTimeOffset readAt = timeProvider.GetUtcNow();
        int changedCount = 0;
        foreach (NotificationRecord notification in notifications)
        {
            if (notification.MarkRead(readAt))
            {
                changedCount++;
            }
        }

        if (changedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return changedCount;
    }
}
