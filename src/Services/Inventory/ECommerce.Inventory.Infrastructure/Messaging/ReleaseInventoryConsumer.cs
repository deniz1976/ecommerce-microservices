using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Commands.ReleaseInventory;
using ECommerce.Inventory.Application.Inventory;
using MassTransit;

namespace ECommerce.Inventory.Infrastructure.Messaging;

public sealed class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly ICommandHandler<ReleaseInventoryCommand> commandHandler;

    public ReleaseInventoryConsumer(ICommandHandler<ReleaseInventoryCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        return commandHandler.HandleAsync(
            new ReleaseInventoryCommand(
                new InventoryReleaseRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId)),
            context.CancellationToken);
    }
}
