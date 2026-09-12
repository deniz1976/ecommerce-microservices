using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.GetInventoryItems;

public sealed record GetInventoryItemsQuery(IReadOnlyCollection<Guid> ProductIds)
    : IQuery<IReadOnlyCollection<InventoryItemResponse>>;
