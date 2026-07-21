using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogReferenceRepository : ICatalogReferenceRepository
{
    private readonly CatalogDbContext dbContext;

    public CatalogReferenceRepository(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Include(category => category.Translations)
            .Where(category => category.IsActive)
            .OrderBy(category => category.Slug)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Brands
            .AsNoTracking()
            .Where(brand => brand.IsActive)
            .OrderBy(brand => brand.Name)
            .ToArrayAsync(cancellationToken);
    }
}
