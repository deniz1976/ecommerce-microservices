using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class CustomerCancellationWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public CustomerCancellationWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.CustomerCancellation;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing customer-cancellation scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(
            customerId,
            productId,
            10.50m,
            "Customer Cancellation",
            "34000",
            cancellationToken);
        Console.WriteLine($"Customer-cancellation scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync(
            "customer-cancellation inventory reservation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "InventoryReserved",
            deadline,
            cancellationToken);

        OrderResponse cancellation = await context.RequestOrderCancellationAsync(
            order.Id,
            cancellationToken);
        if (cancellation.Status != RuntimeOrderStatus.CancellationRequested)
        {
            throw new InvalidOperationException(
                $"Cancellation request returned unexpected order status '{cancellation.Status}'.");
        }

        await context.WaitForExpectedValueAsync(
            "customer-cancellation order cancellation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "customer-cancellation saga cancellation",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "customer-cancellation late payment refund",
            "ConnectionStrings__PaymentDb",
            "select status from payments where order_id = @order_id",
            order.Id,
            "Refunded",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "customer-cancellation inventory release",
            "ConnectionStrings__InventoryDb",
            "select status from stock_reservations where order_id = @order_id",
            order.Id,
            "Released",
            deadline,
            cancellationToken);
        await context.AssertOrderPresentationAsync(
            order.Id,
            customerId,
            RuntimeOrderStatus.Cancelled,
            "Workflow Check Customer Cancellation",
            "34000",
            cancellationToken);
    }
}
