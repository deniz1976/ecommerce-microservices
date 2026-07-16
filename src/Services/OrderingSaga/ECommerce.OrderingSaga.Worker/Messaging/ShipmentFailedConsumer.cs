using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly OrderWorkflowService workflowService;

    public ShipmentFailedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
