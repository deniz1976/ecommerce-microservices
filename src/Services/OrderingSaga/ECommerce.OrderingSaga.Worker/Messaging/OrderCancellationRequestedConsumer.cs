using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class OrderCancellationRequestedConsumer(ISender sender)
    : IConsumer<OrderCancellationRequested>
{
    public Task Consume(ConsumeContext<OrderCancellationRequested> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<OrderCancellationRequested>(
                context.Message),
            context.CancellationToken);
    }
}
