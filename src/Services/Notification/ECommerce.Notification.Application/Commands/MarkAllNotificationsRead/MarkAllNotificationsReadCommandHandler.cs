using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    private readonly NotificationService notificationService;

    public MarkAllNotificationsReadCommandHandler(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task<int> HandleAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken)
    {
        return notificationService.MarkAllReadAsync(
            command.CustomerId,
            cancellationToken);
    }
}
