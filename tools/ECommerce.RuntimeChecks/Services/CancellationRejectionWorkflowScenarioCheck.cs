using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class CancellationRejectionWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;
    private readonly IWorkflowEventPublisher eventPublisher;

    public CancellationRejectionWorkflowScenarioCheck(
        WorkflowScenarioContext context,
        IWorkflowEventPublisher eventPublisher)
    {
        this.context = context;
        this.eventPublisher = eventPublisher;
    }

    public WorkflowScenario Scenario => WorkflowScenario.CancellationRejection;

    public bool IncludeInAll => false;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing cancellation-rejection scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(
            customerId,
            productId,
            10.50m,
            "Cancellation Rejection",
            "34000",
            cancellationToken);
        Console.WriteLine($"Cancellation-rejection scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync(
            "cancellation-rejection saga completion",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Completed",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "cancellation-rejection order confirmation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Confirmed",
            deadline,
            cancellationToken);

        Console.WriteLine("Publishing a late cancellation request straight to the broker.");
        await eventPublisher.PublishOrderCancellationRequestedAsync(
            order.Id,
            customerId,
            cancellationToken);

        await context.WaitForPositiveValueAsync(
            "cancellation-rejection notification",
            "ConnectionStrings__NotificationDb",
            "select count(*) from notifications where order_id = @order_id and type = 'order.cancellation_rejected'",
            order.Id,
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "cancellation-rejection saga stays completed",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Completed",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "cancellation-rejection order stays confirmed",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Confirmed",
            deadline,
            cancellationToken);
        await context.AssertOrderPresentationAsync(
            order.Id,
            customerId,
            RuntimeOrderStatus.Confirmed,
            "Workflow Check Cancellation Rejection",
            "34000",
            cancellationToken);
    }
}
