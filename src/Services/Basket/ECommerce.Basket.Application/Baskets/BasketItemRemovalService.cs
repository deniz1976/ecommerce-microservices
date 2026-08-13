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
        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);
        if (basket is null)
        {
            return Result<BasketResponse>.Failure(
                new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound));
        }

        basket.RemoveItem(productId);
        await activeBasketStore.SaveAsync(basket, cancellationToken);
        return Result<BasketResponse>.Success(basket.ToResponse());
    }
}
