using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class StoreRepository : IStoreRepository
{
    private readonly CatalogDbContext dbContext;

    public StoreRepository(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Stores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

    public void Add(Store store)
    {
        dbContext.Stores.Add(store);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
