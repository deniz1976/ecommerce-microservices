using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;

public sealed class CreateOrderFromCheckoutCommandHandler
    : ICommandHandler<CreateOrderFromCheckoutCommand, Result<OrderResponse>>
{
    private readonly OrderService orderService;

    public CreateOrderFromCheckoutCommandHandler(OrderService orderService)
    {
        this.orderService = orderService;
    }

    public Task<Result<OrderResponse>> HandleAsync(
        CreateOrderFromCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        return orderService.CreateFromCheckoutAsync(command.Checkout, cancellationToken);
    }
}
