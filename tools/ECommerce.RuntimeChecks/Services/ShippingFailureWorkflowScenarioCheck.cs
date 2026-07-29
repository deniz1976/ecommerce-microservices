using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class ShippingFailureWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public ShippingFailureWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.ShippingFailure;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing shipping-failure scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(customerId, productId, 10.50m, "Shipping Failure", "00000", cancellationToken);
        Console.WriteLine($"Shipping-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync("shipping-failure order cancellation", "ConnectionStrings__OrderingDb", "select status from orders where id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("shipping-failure saga cancellation", "ConnectionStrings__OrderingSagaDb", "select status from order_workflows where order_id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("failed shipment", "ConnectionStrings__ShippingDb", "select status from shipments where order_id = @order_id", order.Id, "Failed", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("shipping-failure payment compensation", "ConnectionStrings__PaymentDb", "select status from payments where order_id = @order_id", order.Id, "Refunded", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("shipping-failure inventory compensation", "ConnectionStrings__InventoryDb", "select status from stock_reservations where order_id = @order_id", order.Id, "Released", deadline, cancellationToken);
        await context.WaitForMinimumValueAsync("shipping-failure notifications", "ConnectionStrings__NotificationDb", "select count(*) from notifications where order_id = @order_id", order.Id, 3, deadline, cancellationToken);
        await context.AssertOrderPresentationAsync(order.Id, customerId, RuntimeOrderStatus.Cancelled, "Workflow Check Shipping Failure", "00000", cancellationToken);
    }
}
