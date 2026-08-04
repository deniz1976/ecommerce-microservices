using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.RequestOrderCancellation;

public sealed record RequestOrderCancellationCommand(
    Guid OrderId,
    Guid CorrelationId,
    Guid? CausationId) : ICommand<Result<OrderResponse>>;
