using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class SuccessWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public SuccessWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.Success;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing success scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        OrderResponse order = await context.CreateOrderAsync(customerId, productId, 10.50m, "Success", "34000", cancellationToken);
        Console.WriteLine($"Success scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync("success order confirmation", "ConnectionStrings__OrderingDb", "select status from orders where id = @order_id", order.Id, "Confirmed", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("success saga completion", "ConnectionStrings__OrderingSagaDb", "select status from order_workflows where order_id = @order_id", order.Id, "Completed", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("success payment authorization", "ConnectionStrings__PaymentDb", "select status from payments where order_id = @order_id", order.Id, "Authorized", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("success shipment creation", "ConnectionStrings__ShippingDb", "select status from shipments where order_id = @order_id", order.Id, "Created", deadline, cancellationToken);
        await context.WaitForPositiveValueAsync("success notification", "ConnectionStrings__NotificationDb", "select count(*) from notifications where order_id = @order_id", order.Id, deadline, cancellationToken);
    }
}
