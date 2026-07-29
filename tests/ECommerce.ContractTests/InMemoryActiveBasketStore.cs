using ECommerce.Basket.Application.Baskets;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.ContractTests;

public sealed class InMemoryActiveBasketStore : IActiveBasketStore
{
    public BasketEntity? SavedBasket { get; private set; }

    public Task<BasketEntity?> GetAsync(Guid customerId, CancellationToken cancellationToken) =>
        Task.FromResult(SavedBasket?.CustomerId == customerId ? SavedBasket : null);

    public Task SaveAsync(BasketEntity basket, CancellationToken cancellationToken)
    {
        SavedBasket = basket;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid customerId, CancellationToken cancellationToken)
    {
        SavedBasket = null;
        return Task.CompletedTask;
    }
}
