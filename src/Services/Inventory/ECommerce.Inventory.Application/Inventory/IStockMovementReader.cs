using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Inventory.Application.Inventory;

public interface IStockMovementReader
{
    Task<PagedResult<StockMovementResponse>> SearchAsync(
        StockMovementListCriteria criteria,
        CancellationToken cancellationToken);
}
