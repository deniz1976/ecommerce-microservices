using ECommerce.Notification.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Notification.Api.Hubs;

public sealed class SignalRLiveNotificationPublisher : ILiveNotificationPublisher
{
    private readonly IHubContext<NotificationsHub> hubContext;

    public SignalRLiveNotificationPublisher(IHubContext<NotificationsHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public Task PublishAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(NotificationsHub.CustomerGroupName(notification.CustomerId))
            .SendAsync("notificationReceived", notification, cancellationToken);
    }
}
