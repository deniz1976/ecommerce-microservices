using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.GetInventoryItem;

public sealed record GetInventoryItemQuery(Guid ProductId)
    : IQuery<InventoryItemResponse?>;
