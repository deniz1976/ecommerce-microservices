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

        IQueryable<ManagedStoreResponse> projection = stores.Select(store =>
            new ManagedStoreResponse(
                store.Id,
                store.OwnerUserId,
                store.Name,
                store.Slug,
                store.CreatedAt,
                store.UpdatedAt));

        projection = ApplyOrdering(
            projection,
            criteria.SortBy,
            criteria.SortDescending);

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

    private static IQueryable<ManagedStoreResponse> ApplyOrdering(
        IQueryable<ManagedStoreResponse> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            ManagedStoreSortFields.Slug => descending
                ? query.OrderByDescending(item => item.Slug).ThenBy(item => item.Id)
                : query.OrderBy(item => item.Slug).ThenBy(item => item.Id),
            ManagedStoreSortFields.CreatedAt => descending
                ? query.OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.CreatedAt).ThenBy(item => item.Id),
            ManagedStoreSortFields.UpdatedAt => descending
                ? query.OrderByDescending(item => item.UpdatedAt).ThenBy(item => item.Id)
                : query.OrderBy(item => item.UpdatedAt).ThenBy(item => item.Id),
            _ => descending
                ? query.OrderByDescending(item => item.Name).ThenBy(item => item.Id)
                : query.OrderBy(item => item.Name).ThenBy(item => item.Id)
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
