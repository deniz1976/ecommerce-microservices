using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.UnitTests;

internal sealed class FakeNotificationRepository :
    IRepository<NotificationRecord, Guid>,
    IUnitOfWork,
    INotificationReader
{
    public NotificationRecord? Notification { get; private set; }

    public int SaveChangesCount { get; private set; }

    public Task<NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        NotificationChannel channel,
        CancellationToken cancellationToken)
    {
        NotificationRecord? notification = Notification is not null &&
            Notification.SourceMessageId == sourceMessageId &&
            Notification.Channel == channel
                ? Notification
                : null;

        return Task.FromResult(notification);
    }

    public void Add(NotificationRecord notification)
    {
        Notification = notification;
    }

    public Task<NotificationRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Notification?.Id == id ? Notification : null);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
