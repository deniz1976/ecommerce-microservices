using ECommerce.BuildingBlocks.Contracts.Cqrs;

namespace ECommerce.Ordering.Application.Commands.ChangeOrderStatus;

public sealed record ChangeOrderStatusCommand(
    Guid OrderId,
    Guid CustomerId,
    OrderStatusChange Change,
    string? ReasonCode = null) : ICommand;
