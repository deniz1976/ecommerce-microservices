using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.UnitTests;

internal sealed class FakeNotificationRepository : INotificationRepository
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

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
