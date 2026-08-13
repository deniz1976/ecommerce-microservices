using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.AddBasketItem;

public sealed class AddBasketItemCommandHandler
    : ICommandHandler<AddBasketItemCommand, Result<BasketResponse>>
{
    private readonly BasketMutationService basketMutationService;

    public AddBasketItemCommandHandler(BasketMutationService basketMutationService)
    {
        this.basketMutationService = basketMutationService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        AddBasketItemCommand command,
        CancellationToken cancellationToken)
    {
        return basketMutationService.AddItemAsync(
            command.CustomerId,
            command.Request,
            cancellationToken);
    }
}
