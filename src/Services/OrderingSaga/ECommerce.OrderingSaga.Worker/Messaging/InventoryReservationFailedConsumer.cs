using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailed>
{
    private readonly ICommandHandler<
        ProcessWorkflowEventCommand<InventoryReservationFailed>> commandHandler;

    public InventoryReservationFailedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<InventoryReservationFailed>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<InventoryReservationFailed>(context.Message),
            context.CancellationToken);
    }
}
