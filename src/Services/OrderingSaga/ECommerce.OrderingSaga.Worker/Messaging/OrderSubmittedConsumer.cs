using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly ISender sender;

    public OrderSubmittedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<OrderSubmitted>(context.Message),
            context.CancellationToken);
    }
}
