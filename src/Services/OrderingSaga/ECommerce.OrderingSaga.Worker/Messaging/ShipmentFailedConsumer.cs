using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly ISender sender;

    public ShipmentFailedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<ShipmentFailed>(context.Message),
            context.CancellationToken);
    }
}
