using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.ClearBasket;

public sealed class ClearBasketCommandHandler
    : ICommandHandler<ClearBasketCommand, Result>
{
    private readonly BasketMutationService basketMutationService;

    public ClearBasketCommandHandler(BasketMutationService basketMutationService)
    {
        this.basketMutationService = basketMutationService;
    }

    public Task<Result> HandleAsync(
        ClearBasketCommand command,
        CancellationToken cancellationToken)
    {
        return basketMutationService.ClearAsync(command.CustomerId, cancellationToken);
    }
}
