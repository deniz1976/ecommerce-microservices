using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using MassTransit;

namespace ECommerce.OrderingSaga.Worker.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly OrderWorkflowService workflowService;

    public PaymentAuthorizedConsumer(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        return workflowService.HandleAsync(context.Message, context.CancellationToken);
    }
}
