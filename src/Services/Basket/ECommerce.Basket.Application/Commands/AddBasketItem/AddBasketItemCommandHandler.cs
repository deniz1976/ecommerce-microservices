using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.AddBasketItem;

public sealed class AddBasketItemCommandHandler
    : ICommandHandler<AddBasketItemCommand, Result<BasketResponse>>
{
    private readonly BasketItemAdditionService basketMutationService;

    public AddBasketItemCommandHandler(BasketItemAdditionService basketMutationService)
    {
        this.basketMutationService = basketMutationService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        AddBasketItemCommand command,
        CancellationToken cancellationToken)
    {
        return basketMutationService.AddAsync(
            command.CustomerId,
            command.Request,
            cancellationToken);
    }
}
