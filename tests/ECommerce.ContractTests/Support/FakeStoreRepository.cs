using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

internal sealed class FakeStoreRepository :
    IRepository<Store, Guid>,
    IUnitOfWork,
    IStoreReader
{
    private readonly Dictionary<Guid, Store> stores;

    public FakeStoreRepository(params Store[] stores)
    {
        this.stores = stores.ToDictionary(x => x.Id);
    }

    public Store? Added { get; private set; }

    public Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(stores.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Store>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<Store>>(stores.Values.Where(x => x.OwnerUserId == ownerUserId).ToArray());

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        Task.FromResult(stores.Values.Any(x => x.Slug == slug));

    public void Add(Store store)
    {
        Added = store;
        stores.Add(store.Id, store);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
