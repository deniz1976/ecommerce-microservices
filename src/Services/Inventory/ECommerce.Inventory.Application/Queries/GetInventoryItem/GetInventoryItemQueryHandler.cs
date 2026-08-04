using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.GetInventoryItem;

public sealed class GetInventoryItemQueryHandler
    : IQueryHandler<GetInventoryItemQuery, InventoryItemResponse?>
{
    private readonly InventoryQueryService queryService;

    public GetInventoryItemQueryHandler(InventoryQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<InventoryItemResponse?> HandleAsync(
        GetInventoryItemQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetAsync(query.ProductId, cancellationToken);
    }
}
