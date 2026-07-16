using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly OrderWorkflowService workflowService;

    public InventoryReservedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
