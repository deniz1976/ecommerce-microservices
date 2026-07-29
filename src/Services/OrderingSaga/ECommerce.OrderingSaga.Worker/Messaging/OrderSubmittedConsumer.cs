using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly ICommandHandler<ProcessWorkflowEventCommand<OrderSubmitted>> commandHandler;

    public OrderSubmittedConsumer(
        ICommandHandler<ProcessWorkflowEventCommand<OrderSubmitted>> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        return commandHandler.HandleAsync(
            new ProcessWorkflowEventCommand<OrderSubmitted>(context.Message),
            context.CancellationToken);
    }
}
