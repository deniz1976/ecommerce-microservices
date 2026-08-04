using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ISender sender;

    public ShipmentCreatedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<ShipmentCreated>(context.Message),
            context.CancellationToken);
    }
}
