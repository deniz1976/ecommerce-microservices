using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.UpsertInventoryItem;

public sealed class UpsertInventoryItemCommandHandler
    : ICommandHandler<UpsertInventoryItemCommand, InventoryItemResponse>
{
    private readonly InventoryService inventoryService;

    public UpsertInventoryItemCommandHandler(InventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task<InventoryItemResponse> HandleAsync(
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        return inventoryService.UpsertAsync(command.Request, cancellationToken);
    }
}
