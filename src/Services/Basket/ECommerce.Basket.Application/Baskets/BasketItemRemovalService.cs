using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketItemRemovalService(IActiveBasketStore activeBasketStore)
{
    public async Task<Result<BasketResponse>> RemoveAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        Result<BasketEntity?> storeResult = await activeBasketStore.GetAsync(customerId, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result<BasketResponse>.Failure(storeResult.Error!);
        }

        BasketEntity? basket = storeResult.Value;
        if (basket is null)
        {
            return Result<BasketResponse>.Failure(
                new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound));
        }

        basket.RemoveItem(productId);

        Result saveResult = await activeBasketStore.SaveAsync(basket, cancellationToken);

        return saveResult.IsFailure
            ? Result<BasketResponse>.Failure(saveResult.Error!)
            : Result<BasketResponse>.Success(basket.ToResponse());
    }
}
