using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;

namespace ECommerce.ContractTests;

internal sealed class StubNotificationDeliveryProvider(NotificationDispatchChannel channel)
    : INotificationDeliveryProvider
{
    public NotificationDispatchChannel Channel { get; } = channel;

    public Task<NotificationDeliveryResult> DeliverAsync(
        NotificationDelivery delivery,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(NotificationDeliveryResult.Delivered);
    }
}
