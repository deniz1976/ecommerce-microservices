using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly OrderWorkflowService workflowService;

    public OrderSubmittedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
