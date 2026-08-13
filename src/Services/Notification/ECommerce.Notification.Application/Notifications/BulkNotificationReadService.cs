using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed class BulkNotificationReadService(
    INotificationReader notificationReader,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<int> MarkAllReadAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<NotificationRecord> notifications =
            await notificationReader.GetUnreadByCustomerAsync(customerId, cancellationToken);
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
