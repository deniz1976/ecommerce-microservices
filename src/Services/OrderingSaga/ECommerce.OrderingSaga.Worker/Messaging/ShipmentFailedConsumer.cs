using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<ShipmentFailed>> commandHandler;

    public ShipmentFailedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<ShipmentFailed>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<ShipmentFailed>(context.Message),
            context.CancellationToken);
    }
}
