using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.SearchManagedInventory;

public sealed record SearchManagedInventoryQuery(ManagedInventoryListCriteria Criteria)
    : IQuery<PagedResult<InventoryItemResponse>>;
