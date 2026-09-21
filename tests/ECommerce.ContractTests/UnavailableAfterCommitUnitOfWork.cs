using ECommerce.BuildingBlocks.Contracts.Persistence;

namespace ECommerce.ContractTests;

public sealed class UnavailableAfterCommitUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork inner;
    private readonly InMemoryActiveBasketStore store;

    public UnavailableAfterCommitUnitOfWork(IUnitOfWork inner, InMemoryActiveBasketStore store)
    {
        this.inner = inner;
        this.store = store;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await inner.SaveChangesAsync(cancellationToken);
        store.IsUnavailable = true;
    }
}
