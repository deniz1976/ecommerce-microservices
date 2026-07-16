using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly OrderWorkflowService workflowService;

    public ShipmentCreatedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
