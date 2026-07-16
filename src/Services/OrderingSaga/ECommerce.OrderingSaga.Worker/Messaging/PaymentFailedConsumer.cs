using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly OrderWorkflowService workflowService;

    public PaymentFailedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
