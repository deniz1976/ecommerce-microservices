using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<PaymentFailed>> commandHandler;

    public PaymentFailedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<PaymentFailed>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<PaymentFailed>(context.Message),
            context.CancellationToken);
    }
}
