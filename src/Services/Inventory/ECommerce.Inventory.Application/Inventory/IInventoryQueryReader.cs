using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Inventory.Application.Inventory;

public interface IInventoryQueryReader
{
    Task<InventoryItemResponse?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InventoryItemResponse>> GetManyAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task<PagedResult<InventoryItemResponse>> SearchAsync(
        ManagedInventoryListCriteria criteria,
        CancellationToken cancellationToken);
}
