using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailed>
{
    private readonly ISender sender;

    public InventoryReservationFailedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<InventoryReservationFailed>(context.Message),
            context.CancellationToken);
    }
}
