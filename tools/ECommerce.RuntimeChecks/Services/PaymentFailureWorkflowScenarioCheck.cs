using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class PaymentFailureWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public PaymentFailureWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.PaymentFailure;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing payment-failure scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(customerId, productId, 0m, "Payment Failure", "34000", cancellationToken);
        Console.WriteLine($"Payment-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync("payment-failure order cancellation", "ConnectionStrings__OrderingDb", "select status from orders where id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("payment-failure saga cancellation", "ConnectionStrings__OrderingSagaDb", "select status from order_workflows where order_id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("failed payment", "ConnectionStrings__PaymentDb", "select status from payments where order_id = @order_id", order.Id, "Failed", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("payment-failure inventory compensation", "ConnectionStrings__InventoryDb", "select status from stock_reservations where order_id = @order_id", order.Id, "Released", deadline, cancellationToken);
        await context.WaitForMinimumValueAsync("payment-failure notifications", "ConnectionStrings__NotificationDb", "select count(*) from notifications where order_id = @order_id", order.Id, 2, deadline, cancellationToken);
    }
}
