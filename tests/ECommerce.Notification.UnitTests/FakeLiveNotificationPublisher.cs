using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.UnitTests;

internal sealed class FakeLiveNotificationPublisher : ILiveNotificationPublisher
{
    public List<NotificationMessage> PublishedMessages { get; } = [];

    public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        PublishedMessages.Add(message);
        return Task.CompletedTask;
    }
}
