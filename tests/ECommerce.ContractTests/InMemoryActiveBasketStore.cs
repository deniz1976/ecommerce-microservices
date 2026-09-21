using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.ContractTests;

public sealed class InMemoryActiveBasketStore : IActiveBasketStore
{
    private static readonly Error Unavailable =
        new(BasketErrorCodes.BasketStoreUnavailable, BasketErrorCodes.BasketStoreUnavailable);

    public BasketEntity? SavedBasket { get; private set; }

    public bool IsUnavailable { get; set; }

    public int DeleteAttemptCount { get; private set; }

    public Task<Result<BasketEntity?>> GetAsync(Guid customerId, CancellationToken cancellationToken) =>
        Task.FromResult(IsUnavailable
            ? Result<BasketEntity?>.Failure(Unavailable)
            : Result<BasketEntity?>.Success(SavedBasket?.CustomerId == customerId ? SavedBasket : null));

    public Task<Result> SaveAsync(BasketEntity basket, CancellationToken cancellationToken)
    {
        if (IsUnavailable)
        {
            return Task.FromResult(Result.Failure(Unavailable));
        }

        SavedBasket = basket;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeleteAsync(Guid customerId, CancellationToken cancellationToken)
    {
        DeleteAttemptCount++;

        if (IsUnavailable)
        {
            return Task.FromResult(Result.Failure(Unavailable));
        }

        SavedBasket = null;
        return Task.FromResult(Result.Success());
    }
}
