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

    public Task<IReadOnlyCollection<InventoryItemResponse>> GetManyAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        Guid[] normalized = productIds
            .Where(productId => productId != Guid.Empty)
            .Distinct()
            .Take(ManagedInventoryQueryLimits.MaxBatchProductIds)
            .ToArray();

        return normalized.Length == 0
            ? Task.FromResult<IReadOnlyCollection<InventoryItemResponse>>([])
            : inventoryReader.GetManyAsync(normalized, cancellationToken);
    }

    public Task<PagedResult<InventoryItemResponse>> SearchAsync(
        ManagedInventoryListCriteria criteria,
        CancellationToken cancellationToken) =>
        inventoryReader.SearchAsync(criteria, cancellationToken);
}
