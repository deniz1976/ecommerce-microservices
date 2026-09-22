using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ShipInventory;

public sealed record ShipInventoryCommand(InventoryShipmentRequest Request) : ICommand;
