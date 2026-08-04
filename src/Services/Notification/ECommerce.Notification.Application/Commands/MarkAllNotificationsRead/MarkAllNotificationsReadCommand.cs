using ECommerce.BuildingBlocks.Contracts.Cqrs;

namespace ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(Guid CustomerId) : ICommand<int>;
