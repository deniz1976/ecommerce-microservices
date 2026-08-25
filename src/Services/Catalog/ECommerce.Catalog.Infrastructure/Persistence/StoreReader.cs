using ECommerce.Catalog.Application.Stores;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class StoreReader : IStoreReader, IManagedStoreReader
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

    public async Task<PagedResult<ManagedStoreResponse>> SearchAsync(
        ManagedStoreListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<Store> stores = dbContext.Stores.AsNoTracking();

        if (criteria.OwnerUserId.HasValue)
        {
            stores = stores.Where(store =>
                store.OwnerUserId == criteria.OwnerUserId.Value);
        }

        string? searchPattern = CreateSearchPattern(criteria.Search);
        if (searchPattern is not null)
        {
            stores = stores.Where(store =>
                EF.Functions.ILike(store.Name, searchPattern, "\\") ||
                EF.Functions.ILike(store.Slug, searchPattern, "\\"));
        }

        long totalCount = await stores.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(
            criteria.PageNumber,
            pageSize,
            totalCount);

        stores = ApplyOrdering(
            stores,
            criteria.SortBy,
            criteria.SortDescending);

        IQueryable<ManagedStoreResponse> projection = stores.Select(store =>
            new ManagedStoreResponse(
                store.Id,
                store.OwnerUserId,
                store.Name,
                store.Slug,
                store.CreatedAt,
                store.UpdatedAt));

        ManagedStoreResponse[] items = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<ManagedStoreResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<Store> ApplyOrdering(
        IQueryable<Store> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            ManagedStoreSortFields.Slug => descending
                ? query.OrderByDescending(store => store.Slug).ThenBy(store => store.Id)
                : query.OrderBy(store => store.Slug).ThenBy(store => store.Id),
            ManagedStoreSortFields.CreatedAt => descending
                ? query.OrderByDescending(store => store.CreatedAt).ThenBy(store => store.Id)
                : query.OrderBy(store => store.CreatedAt).ThenBy(store => store.Id),
            ManagedStoreSortFields.UpdatedAt => descending
                ? query.OrderByDescending(store => store.UpdatedAt).ThenBy(store => store.Id)
                : query.OrderBy(store => store.UpdatedAt).ThenBy(store => store.Id),
            _ => descending
                ? query.OrderByDescending(store => store.Name).ThenBy(store => store.Id)
                : query.OrderBy(store => store.Name).ThenBy(store => store.Id)
        };
    }

    private static int NormalizePageSize(int pageSize) =>
        pageSize is <= 0 or > ManagedStoreQueryLimits.MaxPageSize
            ? ManagedStoreQueryLimits.DefaultPageSize
            : pageSize;

    private static int NormalizePageNumber(
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        long totalPages = Math.Max(
            1,
            (long)Math.Ceiling(totalCount / (double)pageSize));
        return (int)Math.Min(Math.Max(1, pageNumber), totalPages);
    }

    private static string? CreateSearchPattern(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalized = search.Trim();
        if (normalized.Length > ManagedStoreQueryLimits.MaxSearchLength)
        {
            normalized = normalized[..ManagedStoreQueryLimits.MaxSearchLength];
        }

        string escaped = normalized
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        return $"%{escaped}%";
    }
}
