using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationReadService(
    IRepository<NotificationRecord, Guid> notificationRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<Result<NotificationMessage>> MarkReadAsync(
        Guid customerId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        NotificationRecord? notification = await notificationRepository.GetByIdAsync(
            notificationId,
            cancellationToken);
        if (notification is null || notification.CustomerId != customerId)
        {
            return Result<NotificationMessage>.Failure(new Error(
                ErrorCodes.NotificationNotFound,
                ErrorCodes.NotificationNotFound));
        }

        if (notification.MarkRead(timeProvider.GetUtcNow()))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<NotificationMessage>.Success(NotificationMapper.ToMessage(notification));
    }
}
