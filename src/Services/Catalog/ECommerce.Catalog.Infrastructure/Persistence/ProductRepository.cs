using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<Product>> SearchAsync(ProductListQuery query, CancellationToken cancellationToken)
    {
        int pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        int pageSize = query.PageSize is <= 0 or > 100 ? 20 : query.PageSize;

        IQueryable<Product> products = dbContext.Products
            .AsNoTracking()
            .Include(x => x.Translations)
            .Include(x => x.Images)
            .Include(x => x.Category)
                .ThenInclude(x => x!.Translations)
            .Include(x => x.Brand);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            products = products.Where(x => x.Sku.Contains(search) || x.Translations.Any(t => t.Name.Contains(search)));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(x => x.CategoryId == query.CategoryId);
        }

        if (query.BrandId.HasValue)
        {
            products = products.Where(x => x.BrandId == query.BrandId);
        }

        if (query.Status.HasValue)
        {
            products = products.Where(x => x.Status == query.Status);
        }

        products = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDescending ? products.OrderByDescending(x => x.Price) : products.OrderBy(x => x.Price),
            "createdat" => query.SortDescending ? products.OrderByDescending(x => x.CreatedAt) : products.OrderBy(x => x.CreatedAt),
            _ => query.SortDescending ? products.OrderByDescending(x => x.Sku) : products.OrderBy(x => x.Sku)
        };

        long totalCount = await products.LongCountAsync(cancellationToken);
        Product[] items = await products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Product>(items, pageNumber, pageSize, totalCount);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Products
            .Include(x => x.Translations)
            .Include(x => x.Images)
            .Include(x => x.Category)
                .ThenInclude(x => x!.Translations)
            .Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return dbContext.Categories.AnyAsync(x => x.Id == categoryId && x.IsActive, cancellationToken);
    }

    public Task<bool> BrandExistsAsync(Guid brandId, CancellationToken cancellationToken)
    {
        return dbContext.Brands.AnyAsync(x => x.Id == brandId && x.IsActive, cancellationToken);
    }

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
