using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler
    : ICommandHandler<CreateNotificationCommand, NotificationMessage>
{
    private readonly NotificationService notificationService;

    public CreateNotificationCommandHandler(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task<NotificationMessage> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        return notificationService.CreateAsync(command.Request, cancellationToken);
    }
}
