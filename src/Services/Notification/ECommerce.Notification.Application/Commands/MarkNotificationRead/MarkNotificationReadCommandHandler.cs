using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler
    : ICommandHandler<MarkNotificationReadCommand, Result<NotificationMessage>>
{
    private readonly NotificationReadStateService service;

    public MarkNotificationReadCommandHandler(NotificationReadStateService service)
    {
        this.service = service;
    }

    public Task<Result<NotificationMessage>> HandleAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken)
    {
        return service.MarkReadAsync(
            command.CustomerId,
            command.NotificationId,
            cancellationToken);
    }
}
