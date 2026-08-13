using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : ICommandHandler<CreateOrderCommand, Result<OrderResponse>>
{
    private readonly OrderCreationService orderService;

    public CreateOrderCommandHandler(OrderCreationService orderService)
    {
        this.orderService = orderService;
    }

    public Task<Result<OrderResponse>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        return orderService.CreateAsync(
            command.Request,
            command.CorrelationId,
            command.CausationId,
            cancellationToken);
    }
}
