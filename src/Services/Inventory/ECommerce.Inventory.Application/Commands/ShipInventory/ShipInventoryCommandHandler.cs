using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ShipInventory;

public sealed class ShipInventoryCommandHandler : ICommandHandler<ShipInventoryCommand>
{
    private readonly InventoryShipmentService inventoryService;

    public ShipInventoryCommandHandler(InventoryShipmentService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task HandleAsync(
        ShipInventoryCommand command,
        CancellationToken cancellationToken)
    {
        return inventoryService.ShipAsync(command.Request, cancellationToken);
    }
}
