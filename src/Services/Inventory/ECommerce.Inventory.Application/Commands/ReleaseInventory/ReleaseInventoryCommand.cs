using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ReleaseInventory;

public sealed record ReleaseInventoryCommand(InventoryReleaseRequest Request) : ICommand;
