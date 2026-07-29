using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    Guid CustomerId,
    Guid NotificationId) : ICommand<Result<NotificationMessage>>;
