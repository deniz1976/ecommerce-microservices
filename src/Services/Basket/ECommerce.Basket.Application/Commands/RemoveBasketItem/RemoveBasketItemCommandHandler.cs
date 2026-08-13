using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.RemoveBasketItem;

public sealed class RemoveBasketItemCommandHandler
    : ICommandHandler<RemoveBasketItemCommand, Result<BasketResponse>>
{
    private readonly BasketMutationService basketMutationService;

    public RemoveBasketItemCommandHandler(BasketMutationService basketMutationService)
    {
        this.basketMutationService = basketMutationService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        RemoveBasketItemCommand command,
        CancellationToken cancellationToken)
    {
        return basketMutationService.RemoveItemAsync(
            command.CustomerId,
            command.ProductId,
            cancellationToken);
    }
}
