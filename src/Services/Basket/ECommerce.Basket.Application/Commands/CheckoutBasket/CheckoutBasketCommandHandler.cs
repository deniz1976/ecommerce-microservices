using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.CheckoutBasket;

public sealed class CheckoutBasketCommandHandler
    : ICommandHandler<CheckoutBasketCommand, Result<CheckoutBasketResponse>>
{
    private readonly BasketService basketService;

    public CheckoutBasketCommandHandler(BasketService basketService)
    {
        this.basketService = basketService;
    }

    public Task<Result<CheckoutBasketResponse>> HandleAsync(
        CheckoutBasketCommand command,
        CancellationToken cancellationToken)
    {
        return basketService.CheckoutAsync(
            command.CustomerId,
            command.Request,
            cancellationToken);
    }
}
