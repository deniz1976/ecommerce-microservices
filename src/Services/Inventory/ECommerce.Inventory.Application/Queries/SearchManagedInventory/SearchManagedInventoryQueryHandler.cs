using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.SearchManagedInventory;

public sealed class SearchManagedInventoryQueryHandler
    : IQueryHandler<SearchManagedInventoryQuery, PagedResult<InventoryItemResponse>>
{
    private readonly InventoryQueryService queryService;

    public SearchManagedInventoryQueryHandler(InventoryQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<InventoryItemResponse>> HandleAsync(
        SearchManagedInventoryQuery query,
        CancellationToken cancellationToken) =>
        queryService.SearchAsync(query.Criteria, cancellationToken);
}
