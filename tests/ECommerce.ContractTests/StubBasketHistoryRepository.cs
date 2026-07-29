using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Persistence;

namespace ECommerce.ContractTests;

public sealed class StubBasketHistoryRepository :
    IRepository<BasketCheckoutSnapshot, Guid>,
    IUnitOfWork
{
    public BasketCheckoutSnapshot? AddedSnapshot { get; private set; }

    public Task<BasketCheckoutSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(AddedSnapshot?.Id == id ? AddedSnapshot : null);
    }

    public void Add(BasketCheckoutSnapshot snapshot)
    {
        AddedSnapshot = snapshot;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
