using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class StockMovementReader : IStockMovementReader
{
    private readonly InventoryDbContext dbContext;

    public StockMovementReader(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<StockMovementResponse>> SearchAsync(
        StockMovementListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = criteria.PageSize is <= 0 or > ManagedInventoryQueryLimits.MaxPageSize
            ? ManagedInventoryQueryLimits.DefaultPageSize
            : criteria.PageSize;
        IQueryable<StockMovement> movements = dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.ProductId == criteria.ProductId);

        if (criteria.OrderId.HasValue)
        {
            movements = movements.Where(movement => movement.OrderId == criteria.OrderId.Value);
        }

        if (criteria.Type.HasValue)
        {
            StockMovementType type = (StockMovementType)criteria.Type.Value;
            movements = movements.Where(movement => movement.Type == type);
        }

        long totalCount = await movements.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(criteria.PageNumber, pageSize, totalCount);
        StockMovementResponse[] items = await movements
            .OrderByDescending(movement => movement.OccurredAt)
            .ThenByDescending(movement => movement.Id)
            .Select(movement => new StockMovementResponse(
                movement.Id,
                movement.ProductId,
                (StockMovementKind)movement.Type,
                movement.Quantity,
                movement.QuantityOnHandBefore,
                movement.QuantityOnHandAfter,
                movement.ReservedQuantityBefore,
                movement.ReservedQuantityAfter,
                movement.OrderId,
                movement.OccurredAt))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<StockMovementResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static int NormalizePageNumber(int pageNumber, int pageSize, long totalCount)
    {
        long totalPages = Math.Max(1, (long)Math.Ceiling(totalCount / (double)pageSize));
        return (int)Math.Min(Math.Max(1, pageNumber), totalPages);
    }
}
