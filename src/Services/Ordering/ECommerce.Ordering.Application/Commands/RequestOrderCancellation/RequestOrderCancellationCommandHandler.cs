using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.RequestOrderCancellation;

public sealed class RequestOrderCancellationCommandHandler(
    OrderCancellationService cancellationService)
    : ICommandHandler<RequestOrderCancellationCommand, Result<OrderResponse>>
{
    public Task<Result<OrderResponse>> HandleAsync(
        RequestOrderCancellationCommand command,
        CancellationToken cancellationToken)
    {
        return cancellationService.RequestAsync(
            command.OrderId,
            command.CorrelationId,
            command.CausationId,
            cancellationToken);
    }
}
