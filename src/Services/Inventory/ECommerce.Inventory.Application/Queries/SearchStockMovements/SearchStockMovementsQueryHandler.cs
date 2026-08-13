using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.SearchStockMovements;

public sealed class SearchStockMovementsQueryHandler
    : IQueryHandler<SearchStockMovementsQuery, PagedResult<StockMovementResponse>>
{
    private readonly StockMovementQueryService queryService;

    public SearchStockMovementsQueryHandler(StockMovementQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<StockMovementResponse>> HandleAsync(
        SearchStockMovementsQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.SearchAsync(query.Criteria, cancellationToken);
    }
}
