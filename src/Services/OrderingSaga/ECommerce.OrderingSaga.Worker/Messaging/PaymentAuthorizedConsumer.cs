using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<PaymentAuthorized>> commandHandler;

    public PaymentAuthorizedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<PaymentAuthorized>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<PaymentAuthorized>(context.Message),
            context.CancellationToken);
    }
}
