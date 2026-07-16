using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class WorkflowCheckRunner
{
    private readonly GatewayWorkflowClient gatewayClient;
    private readonly PostgresWorkflowProbe workflowProbe;
    private readonly RuntimeCheckOptions options;

    public WorkflowCheckRunner(HttpClient httpClient, RuntimeCheckOptions options)
    {
        gatewayClient = new GatewayWorkflowClient(httpClient, options.GatewayBaseUri, options.AccessToken);
        workflowProbe = new PostgresWorkflowProbe();
        this.options = options;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting runtime workflow scenario: {options.Scenario}");

        string suffix = Guid.NewGuid().ToString("N")[..12];
        Console.WriteLine("Registering an isolated workflow-check user.");
        UserResponse user = await gatewayClient.RegisterUserAsync(
            new CreateUserRequest(
                $"workflow-{suffix}@example.com",
                $"Workflow Check {suffix}",
                "WorkflowCheck123!"),
            cancellationToken);
        Console.WriteLine($"Workflow-check user registered: {user.Id}");

        switch (options.Scenario)
        {
            case WorkflowScenario.All:
                await RunSuccessAsync(user.Id, cancellationToken);
                await RunInventoryFailureAsync(user.Id, cancellationToken);
                await RunPaymentFailureAsync(user.Id, cancellationToken);
                await RunShippingFailureAsync(user.Id, cancellationToken);
                break;
            case WorkflowScenario.Success:
                await RunSuccessAsync(user.Id, cancellationToken);
                break;
            case WorkflowScenario.InventoryFailure:
                await RunInventoryFailureAsync(user.Id, cancellationToken);
                break;
            case WorkflowScenario.PaymentFailure:
                await RunPaymentFailureAsync(user.Id, cancellationToken);
                break;
            case WorkflowScenario.ShippingFailure:
                await RunShippingFailureAsync(user.Id, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(options.Scenario), options.Scenario, "Unsupported workflow scenario.");
        }

        Console.WriteLine($"Runtime workflow scenario completed: {options.Scenario}");
    }

    private async Task RunSuccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing success scenario inventory.");
        Guid productId = Guid.NewGuid();
        await gatewayClient.UpsertInventoryAsync(productId, new UpsertInventoryRequest(25), cancellationToken);

        OrderResponse order = await CreateOrderAsync(customerId, productId, 10.50m, "Success", "34000", cancellationToken);
        Console.WriteLine($"Success scenario order created: {order.Id}");
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(options.Timeout);

        await WaitForExpectedValueAsync(
            "success order confirmation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Confirmed",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "success saga completion",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Completed",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "success payment authorization",
            "ConnectionStrings__PaymentDb",
            "select status from payments where order_id = @order_id",
            order.Id,
            "Authorized",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "success shipment creation",
            "ConnectionStrings__ShippingDb",
            "select status from shipments where order_id = @order_id",
            order.Id,
            "Created",
            deadline,
            cancellationToken);
        await WaitForAsync(
            "success notification",
            () => workflowProbe.HasPositiveValueAsync(
                "ConnectionStrings__NotificationDb",
                "select count(*) from notifications where order_id = @order_id",
                order.Id,
                cancellationToken),
            deadline,
            cancellationToken);
    }

    private async Task RunInventoryFailureAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing inventory-failure scenario.");
        Guid missingProductId = Guid.NewGuid();
        OrderResponse order = await CreateOrderAsync(customerId, missingProductId, 10.50m, "Inventory Failure", "34000", cancellationToken);
        Console.WriteLine($"Inventory-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(options.Timeout);

        await WaitForExpectedValueAsync(
            "inventory-failure order cancellation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "inventory-failure saga cancellation",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "failed inventory reservation",
            "ConnectionStrings__InventoryDb",
            "select status from stock_reservations where order_id = @order_id",
            order.Id,
            "Failed",
            deadline,
            cancellationToken);
    }

    private async Task RunPaymentFailureAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing payment-failure scenario inventory.");
        Guid productId = Guid.NewGuid();
        await gatewayClient.UpsertInventoryAsync(productId, new UpsertInventoryRequest(25), cancellationToken);

        OrderResponse order = await CreateOrderAsync(customerId, productId, 0m, "Payment Failure", "34000", cancellationToken);
        Console.WriteLine($"Payment-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(options.Timeout);

        await WaitForExpectedValueAsync(
            "payment-failure order cancellation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "payment-failure saga cancellation",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "failed payment",
            "ConnectionStrings__PaymentDb",
            "select status from payments where order_id = @order_id",
            order.Id,
            "Failed",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "payment-failure inventory compensation",
            "ConnectionStrings__InventoryDb",
            "select status from stock_reservations where order_id = @order_id",
            order.Id,
            "Released",
            deadline,
            cancellationToken);
        await WaitForAsync(
            "payment-failure notifications",
            () => workflowProbe.HasMinimumValueAsync(
                "ConnectionStrings__NotificationDb",
                "select count(*) from notifications where order_id = @order_id",
                order.Id,
                2,
                cancellationToken),
            deadline,
            cancellationToken);
    }

    private async Task RunShippingFailureAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing shipping-failure scenario inventory.");
        Guid productId = Guid.NewGuid();
        await gatewayClient.UpsertInventoryAsync(productId, new UpsertInventoryRequest(25), cancellationToken);

        OrderResponse order = await CreateOrderAsync(customerId, productId, 10.50m, "Shipping Failure", "00000", cancellationToken);
        Console.WriteLine($"Shipping-failure scenario order created: {order.Id}");
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(options.Timeout);

        await WaitForExpectedValueAsync(
            "shipping-failure order cancellation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "shipping-failure saga cancellation",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            order.Id,
            "Cancelled",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "failed shipment",
            "ConnectionStrings__ShippingDb",
            "select status from shipments where order_id = @order_id",
            order.Id,
            "Failed",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "shipping-failure payment compensation",
            "ConnectionStrings__PaymentDb",
            "select status from payments where order_id = @order_id",
            order.Id,
            "Refunded",
            deadline,
            cancellationToken);
        await WaitForExpectedValueAsync(
            "shipping-failure inventory compensation",
            "ConnectionStrings__InventoryDb",
            "select status from stock_reservations where order_id = @order_id",
            order.Id,
            "Released",
            deadline,
            cancellationToken);
        await WaitForAsync(
            "shipping-failure notifications",
            () => workflowProbe.HasMinimumValueAsync(
                "ConnectionStrings__NotificationDb",
                "select count(*) from notifications where order_id = @order_id",
                order.Id,
                3,
                cancellationToken),
            deadline,
            cancellationToken);
    }

    private Task<OrderResponse> CreateOrderAsync(
        Guid customerId,
        Guid productId,
        decimal unitPrice,
        string scenarioName,
        string postalCode,
        CancellationToken cancellationToken)
    {
        return gatewayClient.CreateOrderAsync(
            new CreateOrderRequest(
                customerId,
                "USD",
                $"Workflow Check {scenarioName}",
                "Runtime Avenue 1",
                "Istanbul",
                "TR",
                postalCode,
                [new CreateOrderItemRequest(productId, $"Workflow Product {scenarioName}", 1, unitPrice, "USD")]),
            cancellationToken);
    }

    private Task WaitForExpectedValueAsync<T>(
        string probeName,
        string connectionName,
        string sql,
        Guid orderId,
        T expectedValue,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        return WaitForAsync(
            probeName,
            () => workflowProbe.HasExpectedValueAsync(
                connectionName,
                sql,
                orderId,
                expectedValue,
                cancellationToken),
            deadline,
            cancellationToken);
    }

    private async Task WaitForAsync(
        string probeName,
        Func<Task<bool>> probe,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Waiting for runtime probe: {probeName}");

        while (DateTimeOffset.UtcNow <= deadline)
        {
            if (await probe())
            {
                Console.WriteLine($"Runtime probe passed: {probeName}");
                return;
            }

            await Task.Delay(options.PollingInterval, cancellationToken);
        }

        throw new TimeoutException($"Runtime probe '{probeName}' did not complete before the configured timeout.");
    }
}
