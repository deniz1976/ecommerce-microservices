using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;

public sealed class CreateOrderFromCheckoutCommandHandler
    : ICommandHandler<CreateOrderFromCheckoutCommand, Result<OrderResponse>>
{
    private readonly CheckoutOrderCreationService orderService;

    public CreateOrderFromCheckoutCommandHandler(CheckoutOrderCreationService orderService)
    {
        this.orderService = orderService;
    }

    public Task<Result<OrderResponse>> HandleAsync(
        CreateOrderFromCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        return orderService.CreateAsync(command.Checkout, cancellationToken);
    }
}
