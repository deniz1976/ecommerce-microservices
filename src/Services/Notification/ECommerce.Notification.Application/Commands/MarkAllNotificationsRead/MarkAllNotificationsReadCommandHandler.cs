using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    private readonly NotificationReadStateService notificationReadStateService;

    public MarkAllNotificationsReadCommandHandler(
        NotificationReadStateService notificationReadStateService)
    {
        this.notificationReadStateService = notificationReadStateService;
    }

    public Task<int> HandleAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken)
    {
        return notificationReadStateService.MarkAllReadAsync(
            command.CustomerId,
            cancellationToken);
    }
}
