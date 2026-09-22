using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Inventory.Application.Commands.ShipInventory;
using ECommerce.Inventory.Application.Inventory;
using MassTransit;
using MediatR;

namespace ECommerce.Inventory.Infrastructure.Messaging;

public sealed class ShipInventoryConsumer : IConsumer<ShipInventory>
{
    private readonly ISender sender;

    public ShipInventoryConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipInventory> context)
    {
        return sender.Send(
            new ShipInventoryCommand(
                new InventoryShipmentRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId)),
            context.CancellationToken);
    }
}
