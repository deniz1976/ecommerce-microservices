using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class WorkflowScenarioContext
{
    private readonly IGatewayWorkflowClient gatewayClient;
    private readonly IWorkflowProbe workflowProbe;
    private readonly RuntimeCheckOptions options;

    public WorkflowScenarioContext(
        IGatewayWorkflowClient gatewayClient,
        IWorkflowProbe workflowProbe,
        RuntimeCheckOptions options)
    {
        this.gatewayClient = gatewayClient;
        this.workflowProbe = workflowProbe;
        this.options = options;
    }

    public DateTimeOffset CreateDeadline() => DateTimeOffset.UtcNow.Add(options.Timeout);

    public Task UpsertInventoryAsync(Guid productId, int availableQuantity, CancellationToken cancellationToken)
    {
        return gatewayClient.UpsertInventoryAsync(
            productId,
            new UpsertInventoryRequest(availableQuantity),
            cancellationToken);
    }

    public Task<RuntimeBasketResponse> AddBasketItemAsync(
        Guid customerId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        return gatewayClient.AddBasketItemAsync(
            customerId,
            new RuntimeAddBasketItemRequest(productId, quantity),
            cancellationToken);
    }

    public Task<RuntimeCheckoutBasketResponse> CheckoutBasketAsync(
        Guid customerId,
        RuntimeCheckoutBasketRequest request,
        CancellationToken cancellationToken)
    {
        return gatewayClient.CheckoutBasketAsync(customerId, request, cancellationToken);
    }

    public Task<OrderResponse> CreateOrderAsync(
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

    public Task<OrderResponse> RequestOrderCancellationAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return gatewayClient.RequestOrderCancellationAsync(orderId, cancellationToken);
    }

    public async Task<OrderResponse> AssertOrderPresentationAsync(
        Guid orderId,
        Guid customerId,
        RuntimeOrderStatus expectedStatus,
        string expectedRecipientName,
        string expectedPostalCode,
        CancellationToken cancellationToken)
    {
        OrderResponse order = await gatewayClient.GetOrderAsync(orderId, cancellationToken);

        if (order.CustomerId != customerId ||
            order.Status != expectedStatus ||
            !string.Equals(order.RecipientName, expectedRecipientName, StringComparison.Ordinal) ||
            !string.Equals(order.AddressLine, "Runtime Avenue 1", StringComparison.Ordinal) ||
            !string.Equals(order.City, "Istanbul", StringComparison.Ordinal) ||
            !string.Equals(order.CountryCode, "TR", StringComparison.Ordinal) ||
            !string.Equals(order.PostalCode, expectedPostalCode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Order {order.Id} presentation contract did not match the expected customer, status, or delivery address.");
        }

        if (order.CreatedAt.Offset != TimeSpan.Zero ||
            order.UpdatedAt.Offset != TimeSpan.Zero ||
            order.UpdatedAt < order.CreatedAt)
        {
            throw new InvalidOperationException(
                $"Order {order.Id} presentation contract returned non-UTC or non-monotonic timestamps.");
        }

        RuntimeOrderStatusHistoryResponse[] history = order.StatusHistory
            .OrderBy(entry => entry.OccurredAt)
            .ToArray();
        if (history.Length == 0 ||
            history[0].Status != RuntimeOrderStatus.Submitted ||
            history[^1].Status != expectedStatus ||
            history.Any(entry => entry.OccurredAt.Offset != TimeSpan.Zero))
        {
            throw new InvalidOperationException(
                $"Order {order.Id} presentation contract returned an incomplete or non-UTC status history.");
        }

        Console.WriteLine($"Runtime probe passed: order presentation contract ({expectedStatus})");
        return order;
    }

    public Task WaitForExpectedValueAsync<T>(
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

    public Task WaitForPositiveValueAsync(
        string probeName,
        string connectionName,
        string sql,
        Guid orderId,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        return WaitForAsync(
            probeName,
            () => workflowProbe.HasPositiveValueAsync(connectionName, sql, orderId, cancellationToken),
            deadline,
            cancellationToken);
    }

    public Task WaitForMinimumValueAsync(
        string probeName,
        string connectionName,
        string sql,
        Guid orderId,
        long minimum,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        return WaitForAsync(
            probeName,
            () => workflowProbe.HasMinimumValueAsync(connectionName, sql, orderId, minimum, cancellationToken),
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
