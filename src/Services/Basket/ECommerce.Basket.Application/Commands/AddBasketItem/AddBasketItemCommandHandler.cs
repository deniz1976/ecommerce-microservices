using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.AddBasketItem;

public sealed class AddBasketItemCommandHandler
    : ICommandHandler<AddBasketItemCommand, Result<BasketResponse>>
{
    private readonly BasketService basketService;

    public AddBasketItemCommandHandler(BasketService basketService)
    {
        this.basketService = basketService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        AddBasketItemCommand command,
        CancellationToken cancellationToken)
    {
        return basketService.AddItemAsync(
            command.CustomerId,
            command.Request,
            cancellationToken);
    }
}
