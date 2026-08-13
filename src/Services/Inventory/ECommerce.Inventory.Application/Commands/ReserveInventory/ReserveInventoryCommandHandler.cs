using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ReserveInventory;

public sealed class ReserveInventoryCommandHandler
    : ICommandHandler<ReserveInventoryCommand, InventoryReservationResult>
{
    private readonly InventoryReservationService inventoryService;

    public ReserveInventoryCommandHandler(InventoryReservationService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task<InventoryReservationResult> HandleAsync(
        ReserveInventoryCommand command,
        CancellationToken cancellationToken)
    {
        return inventoryService.ReserveAsync(command.Request, cancellationToken);
    }
}
