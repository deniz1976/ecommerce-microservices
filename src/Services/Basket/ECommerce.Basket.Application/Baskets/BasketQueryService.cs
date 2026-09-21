using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketQueryService
{
    private readonly IActiveBasketStore activeBasketStore;

    public BasketQueryService(IActiveBasketStore activeBasketStore)
    {
        this.activeBasketStore = activeBasketStore;
    }

    public async Task<Result<BasketResponse>> GetAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        Result<BasketEntity?> storeResult = await activeBasketStore.GetAsync(customerId, cancellationToken);

        if (storeResult.IsFailure)
        {
            return Result<BasketResponse>.Failure(storeResult.Error!);
        }

        BasketEntity? basket = storeResult.Value;

        return basket is null
            ? Result<BasketResponse>.Failure(
                new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound))
            : Result<BasketResponse>.Success(basket.ToResponse());
    }
}
