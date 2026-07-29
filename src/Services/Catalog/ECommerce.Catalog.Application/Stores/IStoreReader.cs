using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

public interface IStoreReader
{
    Task<IReadOnlyCollection<Store>> GetByOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
}
