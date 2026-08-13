using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class StockMovementQueryService
{
    private readonly IStockMovementReader movementReader;

    public StockMovementQueryService(IStockMovementReader movementReader)
    {
        this.movementReader = movementReader;
    }

    public Task<PagedResult<StockMovementResponse>> SearchAsync(
        StockMovementListCriteria criteria,
        CancellationToken cancellationToken)
    {
        return movementReader.SearchAsync(criteria, cancellationToken);
    }
}
