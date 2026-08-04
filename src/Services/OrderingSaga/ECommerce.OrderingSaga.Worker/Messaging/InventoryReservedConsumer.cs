using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly ISender sender;

    public InventoryReservedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<InventoryReserved>(context.Message),
            context.CancellationToken);
    }
}
