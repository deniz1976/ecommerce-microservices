using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.RemoveBasketItem;

public sealed class RemoveBasketItemCommandHandler
    : ICommandHandler<RemoveBasketItemCommand, Result<BasketResponse>>
{
    private readonly BasketService basketService;

    public RemoveBasketItemCommandHandler(BasketService basketService)
    {
        this.basketService = basketService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        RemoveBasketItemCommand command,
        CancellationToken cancellationToken)
    {
        return basketService.RemoveItemAsync(
            command.CustomerId,
            command.ProductId,
            cancellationToken);
    }
}
