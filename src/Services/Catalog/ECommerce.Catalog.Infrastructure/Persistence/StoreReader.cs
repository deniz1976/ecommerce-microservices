using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class StoreReader : IStoreReader
{
    private readonly CatalogDbContext dbContext;

    public StoreReader(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Store>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        return await dbContext.Stores
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
    {
        return dbContext.Stores.AnyAsync(x => x.Slug == slug, cancellationToken);
    }
}
