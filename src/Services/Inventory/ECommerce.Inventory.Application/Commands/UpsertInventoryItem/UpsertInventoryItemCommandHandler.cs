using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Commands.UpsertInventoryItem;

public sealed class UpsertInventoryItemCommandHandler
    : ICommandHandler<UpsertInventoryItemCommand, Result<InventoryItemResponse>>
{
    private readonly InventoryManagementService inventoryService;

    public UpsertInventoryItemCommandHandler(InventoryManagementService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task<Result<InventoryItemResponse>> HandleAsync(
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        return inventoryService.UpsertAsync(
            command.Request,
            command.Access,
            cancellationToken);
    }
}
