using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Inventory.Application.Inventory;
using MassTransit;

namespace ECommerce.Inventory.Infrastructure.Messaging;

public sealed class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly InventoryService inventoryService;

    public ReleaseInventoryConsumer(InventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        return inventoryService.ReleaseAsync(new InventoryReleaseRequest(context.Message.OrderId, context.Message.CustomerId), context.CancellationToken);
    }
}
