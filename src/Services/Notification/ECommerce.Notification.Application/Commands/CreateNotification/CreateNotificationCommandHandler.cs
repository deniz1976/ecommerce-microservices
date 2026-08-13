using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler
    : ICommandHandler<CreateNotificationCommand, NotificationMessage>
{
    private readonly NotificationCreationService notificationCreationService;

    public CreateNotificationCommandHandler(
        NotificationCreationService notificationCreationService)
    {
        this.notificationCreationService = notificationCreationService;
    }

    public Task<NotificationMessage> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        return notificationCreationService.CreateAsync(command.Request, cancellationToken);
    }
}
