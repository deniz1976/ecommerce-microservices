using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailed>
{
    private readonly OrderWorkflowService workflowService;

    public InventoryReservationFailedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
