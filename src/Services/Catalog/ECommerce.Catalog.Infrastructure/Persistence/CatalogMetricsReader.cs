using ECommerce.Catalog.Application.Metrics;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogMetricsReader : ICatalogMetricsReader
{
    private readonly CatalogDbContext dbContext;

    public CatalogMetricsReader(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<CatalogMetricsResponse> ReadAsync(CancellationToken cancellationToken)
    {
        var productMetrics = await dbContext.Products
            .GroupBy(_ => 1)
            .Select(products => new
            {
                Total = products.LongCount(),
                Active = products.Sum(product => product.Status == ProductStatus.Active ? 1L : 0L),
                Draft = products.Sum(product => product.Status == ProductStatus.Draft ? 1L : 0L),
                Inactive = products.Sum(product => product.Status == ProductStatus.Inactive ? 1L : 0L),
                Archived = products.Sum(product => product.Status == ProductStatus.Archived ? 1L : 0L)
            })
            .SingleOrDefaultAsync(cancellationToken);
        long totalStores = await dbContext.Stores.LongCountAsync(cancellationToken);
        long totalCategories = await dbContext.Categories.LongCountAsync(cancellationToken);
        long totalBrands = await dbContext.Brands.LongCountAsync(cancellationToken);

        return new CatalogMetricsResponse(
            productMetrics?.Total ?? 0,
            productMetrics?.Active ?? 0,
            productMetrics?.Draft ?? 0,
            productMetrics?.Inactive ?? 0,
            productMetrics?.Archived ?? 0,
            totalStores,
            totalCategories,
            totalBrands);
    }
}
