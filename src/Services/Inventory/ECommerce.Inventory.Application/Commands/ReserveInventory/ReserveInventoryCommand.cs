using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ReserveInventory;

public sealed record ReserveInventoryCommand(InventoryReservationRequest Request)
    : ICommand<InventoryReservationResult>;
