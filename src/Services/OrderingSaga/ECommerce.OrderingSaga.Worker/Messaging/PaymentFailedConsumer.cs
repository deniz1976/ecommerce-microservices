using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ISender sender;

    public PaymentFailedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<PaymentFailed>(context.Message),
            context.CancellationToken);
    }
}
