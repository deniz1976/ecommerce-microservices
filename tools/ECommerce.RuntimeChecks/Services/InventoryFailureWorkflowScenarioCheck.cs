using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class InventoryFailureWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;

    public InventoryFailureWorkflowScenarioCheck(WorkflowScenarioContext context)
    {
        this.context = context;
    }

    public WorkflowScenario Scenario => WorkflowScenario.InventoryFailure;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing inventory-failure scenario.");
        Guid missingProductId = Guid.NewGuid();
        OrderResponse order = await context.CreateOrderAsync(customerId, missingProductId, 10.50m, "Inventory Failure", "34000", cancellationToken);
        Console.WriteLine($"Inventory-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = context.CreateDeadline();

        await context.WaitForExpectedValueAsync("inventory-failure order cancellation", "ConnectionStrings__OrderingDb", "select status from orders where id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("inventory-failure saga cancellation", "ConnectionStrings__OrderingSagaDb", "select status from order_workflows where order_id = @order_id", order.Id, "Cancelled", deadline, cancellationToken);
        await context.WaitForExpectedValueAsync("failed inventory reservation", "ConnectionStrings__InventoryDb", "select status from stock_reservations where order_id = @order_id", order.Id, "Failed", deadline, cancellationToken);
    }
}
