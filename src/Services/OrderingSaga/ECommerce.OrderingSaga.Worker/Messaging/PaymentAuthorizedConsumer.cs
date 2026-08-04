using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;
using MediatR;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ISender sender;

    public PaymentAuthorizedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        return sender.Send(
            new ProcessWorkflowEventCommand<PaymentAuthorized>(context.Message),
            context.CancellationToken);
    }
}
