using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.UpsertInventoryItem;

public sealed record UpsertInventoryItemCommand(
    UpsertInventoryItemRequest Request,
    InventoryWriteAccess Access)
    : ICommand<Result<InventoryItemResponse>>;
