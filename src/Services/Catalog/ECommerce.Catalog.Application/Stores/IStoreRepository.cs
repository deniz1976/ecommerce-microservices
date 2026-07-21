using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Store>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    void Add(Store store);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
