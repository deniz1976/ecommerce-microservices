using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.UpsertInventoryItem;

public sealed record UpsertInventoryItemCommand(UpsertInventoryItemRequest Request)
    : ICommand<InventoryItemResponse>;
