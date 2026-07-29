using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class PaymentDeclineWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public PaymentDeclineWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.PaymentDecline;

    public bool IncludeInAll => false;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing demo-provider payment-decline scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(
            customerId,
            productId,
            10.50m,
            "Payment Decline",
            "34000",
            cancellationToken);
        Console.WriteLine($"Payment-decline scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync(
            "payment-decline order cancellation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "payment-decline saga cancellation",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "demo-provider declined payment",
            "ConnectionStrings__PaymentDb",
            "select status from payments where order_id = @order_id",
            order.Id,
            "Failed",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "payment-decline inventory compensation",
            "ConnectionStrings__InventoryDb",
            "select status from stock_reservations where order_id = @order_id",
            order.Id,
            "Released",
            deadline,
            cancellationToken);
        await context.WaitForMinimumValueAsync(
            "payment-decline notifications",
            "ConnectionStrings__NotificationDb",
            "select count(*) from notifications where order_id = @order_id",
            order.Id,
            2,
            deadline,
            cancellationToken);
        await context.AssertOrderPresentationAsync(
            order.Id,
            customerId,
            RuntimeOrderStatus.Cancelled,
            "Workflow Check Payment Decline",
            "34000",
            cancellationToken);
    }
}
