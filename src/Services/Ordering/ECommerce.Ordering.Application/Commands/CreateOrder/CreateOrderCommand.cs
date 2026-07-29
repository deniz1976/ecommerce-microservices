using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    CreateOrderRequest Request,
    Guid CorrelationId,
    Guid? CausationId) : ICommand<Result<OrderResponse>>;
