using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class InventoryQueryReader : IInventoryQueryReader
{
    private readonly InventoryDbContext dbContext;

    public InventoryQueryReader(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<InventoryItemResponse?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return dbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .Select(item => new InventoryItemResponse(
                item.ProductId,
                item.QuantityOnHand,
                item.ReservedQuantity,
                item.QuantityOnHand - item.ReservedQuantity,
                item.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItemResponse>> GetManyAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryItems
            .AsNoTracking()
            .Where(item => productIds.Contains(item.ProductId))
            .Select(item => new InventoryItemResponse(
                item.ProductId,
                item.QuantityOnHand,
                item.ReservedQuantity,
                item.QuantityOnHand - item.ReservedQuantity,
                item.UpdatedAt))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<InventoryItemResponse>> SearchAsync(
        ManagedInventoryListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<InventoryItem> items = dbContext.InventoryItems.AsNoTracking();

        if (criteria.ProductId.HasValue)
        {
            items = items.Where(item => item.ProductId == criteria.ProductId.Value);
        }

        int? maximumAvailableQuantity = NormalizeQuantityFilter(
            criteria.MaximumAvailableQuantity);
        if (maximumAvailableQuantity.HasValue)
        {
            items = items.Where(item =>
                item.QuantityOnHand - item.ReservedQuantity <=
                maximumAvailableQuantity.Value);
        }

        long totalCount = await items.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(
            criteria.PageNumber,
            pageSize,
            totalCount);

        items = ApplyOrdering(items, criteria.SortBy, criteria.SortDescending);

        IQueryable<InventoryItemResponse> projection = items.Select(item =>
            new InventoryItemResponse(
                item.ProductId,
                item.QuantityOnHand,
                item.ReservedQuantity,
                item.QuantityOnHand - item.ReservedQuantity,
                item.UpdatedAt));

        InventoryItemResponse[] pageItems = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<InventoryItemResponse>(
            pageItems,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<InventoryItem> ApplyOrdering(
        IQueryable<InventoryItem> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            ManagedInventorySortFields.QuantityOnHand => descending
                ? query.OrderByDescending(item => item.QuantityOnHand).ThenBy(item => item.ProductId)
                : query.OrderBy(item => item.QuantityOnHand).ThenBy(item => item.ProductId),
            ManagedInventorySortFields.ReservedQuantity => descending
                ? query.OrderByDescending(item => item.ReservedQuantity).ThenBy(item => item.ProductId)
                : query.OrderBy(item => item.ReservedQuantity).ThenBy(item => item.ProductId),
            ManagedInventorySortFields.AvailableQuantity => descending
                ? query.OrderByDescending(item => item.AvailableQuantity).ThenBy(item => item.ProductId)
                : query.OrderBy(item => item.AvailableQuantity).ThenBy(item => item.ProductId),
            _ => descending
                ? query.OrderByDescending(item => item.UpdatedAt).ThenBy(item => item.ProductId)
                : query.OrderBy(item => item.UpdatedAt).ThenBy(item => item.ProductId)
        };
    }

    private static int NormalizePageSize(int pageSize) =>
        pageSize is <= 0 or > ManagedInventoryQueryLimits.MaxPageSize
            ? ManagedInventoryQueryLimits.DefaultPageSize
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

    private static int? NormalizeQuantityFilter(int? maximumAvailableQuantity)
    {
        if (!maximumAvailableQuantity.HasValue)
        {
            return null;
        }

        return Math.Clamp(
            maximumAvailableQuantity.Value,
            0,
            ManagedInventoryQueryLimits.MaxQuantityFilter);
    }
}
