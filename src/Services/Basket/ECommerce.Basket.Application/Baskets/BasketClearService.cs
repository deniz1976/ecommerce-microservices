using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketClearService(IActiveBasketStore activeBasketStore)
{
    public async Task<Result> ClearAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        await activeBasketStore.DeleteAsync(customerId, cancellationToken);
        return Result.Success();
    }
}
