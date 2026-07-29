using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.GetInventoryItem;

public sealed class GetInventoryItemQueryHandler
    : IQueryHandler<GetInventoryItemQuery, InventoryItemResponse?>
{
    private readonly InventoryService inventoryService;

    public GetInventoryItemQueryHandler(InventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task<InventoryItemResponse?> HandleAsync(
        GetInventoryItemQuery query,
        CancellationToken cancellationToken)
    {
        return inventoryService.GetItemAsync(query.ProductId, cancellationToken);
    }
}
