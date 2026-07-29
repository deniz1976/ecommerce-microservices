using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : ICommandHandler<CreateOrderCommand, Result<OrderResponse>>
{
    private readonly OrderService orderService;

    public CreateOrderCommandHandler(OrderService orderService)
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
