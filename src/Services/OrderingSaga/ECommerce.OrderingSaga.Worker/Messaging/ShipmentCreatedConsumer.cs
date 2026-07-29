using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<ShipmentCreated>> commandHandler;

    public ShipmentCreatedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<ShipmentCreated>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<ShipmentCreated>(context.Message),
            context.CancellationToken);
    }
}
