using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<InventoryReserved>> commandHandler;

    public InventoryReservedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<InventoryReserved>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<InventoryReserved>(context.Message),
            context.CancellationToken);
    }
}
