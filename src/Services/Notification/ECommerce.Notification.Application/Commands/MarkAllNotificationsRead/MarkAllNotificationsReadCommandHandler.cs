using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    private readonly BulkNotificationReadService notificationReadStateService;

    public MarkAllNotificationsReadCommandHandler(
        BulkNotificationReadService notificationReadStateService)
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
