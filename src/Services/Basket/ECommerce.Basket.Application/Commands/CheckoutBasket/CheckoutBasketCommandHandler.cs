using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.CheckoutBasket;

public sealed class CheckoutBasketCommandHandler
    : ICommandHandler<CheckoutBasketCommand, Result<CheckoutBasketResponse>>
{
    private readonly BasketCheckoutService basketCheckoutService;

    public CheckoutBasketCommandHandler(BasketCheckoutService basketCheckoutService)
    {
        this.basketCheckoutService = basketCheckoutService;
    }

    public Task<Result<CheckoutBasketResponse>> HandleAsync(
        CheckoutBasketCommand command,
        CancellationToken cancellationToken)
    {
        return basketCheckoutService.CheckoutAsync(
            command.CustomerId,
            command.Request,
            cancellationToken);
    }
}
