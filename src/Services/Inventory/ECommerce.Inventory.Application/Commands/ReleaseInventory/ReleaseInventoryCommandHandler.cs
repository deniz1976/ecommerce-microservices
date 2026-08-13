using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.ReleaseInventory;

public sealed class ReleaseInventoryCommandHandler : ICommandHandler<ReleaseInventoryCommand>
{
    private readonly InventoryReleaseService inventoryService;

    public ReleaseInventoryCommandHandler(InventoryReleaseService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task HandleAsync(
        ReleaseInventoryCommand command,
        CancellationToken cancellationToken)
    {
        return inventoryService.ReleaseAsync(command.Request, cancellationToken);
    }
}
