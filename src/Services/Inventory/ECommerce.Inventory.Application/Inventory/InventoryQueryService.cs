using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class InventoryQueryService
{
    private readonly IInventoryQueryReader inventoryReader;

    public InventoryQueryService(IInventoryQueryReader inventoryReader)
    {
        this.inventoryReader = inventoryReader;
    }

    public Task<InventoryItemResponse?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken) =>
        inventoryReader.GetAsync(productId, cancellationToken);

    public Task<PagedResult<InventoryItemResponse>> SearchAsync(
        ManagedInventoryListCriteria criteria,
        CancellationToken cancellationToken) =>
        inventoryReader.SearchAsync(criteria, cancellationToken);
}
