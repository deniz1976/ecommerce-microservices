using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Inventory.Application.Commands.ReleaseInventory;
using ECommerce.Inventory.Application.Inventory;
using MassTransit;
using MediatR;

namespace ECommerce.Inventory.Infrastructure.Messaging;

public sealed class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly ISender sender;

    public ReleaseInventoryConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        return sender.Send(
            new ReleaseInventoryCommand(
                new InventoryReleaseRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId)),
            context.CancellationToken);
    }
}
