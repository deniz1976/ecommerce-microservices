using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.ClearBasket;

public sealed class ClearBasketCommandHandler
    : ICommandHandler<ClearBasketCommand, Result>
{
    private readonly BasketService basketService;

    public ClearBasketCommandHandler(BasketService basketService)
    {
        this.basketService = basketService;
    }

    public Task<Result> HandleAsync(
        ClearBasketCommand command,
        CancellationToken cancellationToken)
    {
        return basketService.ClearAsync(command.CustomerId, cancellationToken);
    }
}
