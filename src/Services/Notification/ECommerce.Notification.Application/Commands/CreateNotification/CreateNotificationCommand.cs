using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Commands.CreateNotification;

public sealed record CreateNotificationCommand(CreateNotificationRequest Request)
    : ICommand<NotificationMessage>;
