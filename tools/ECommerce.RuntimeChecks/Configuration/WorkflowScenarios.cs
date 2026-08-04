namespace ECommerce.RuntimeChecks.Configuration;

public static class WorkflowScenarios
{
    public static WorkflowScenario Parse(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "all" => WorkflowScenario.All,
            "basket-checkout" => WorkflowScenario.BasketCheckout,
            "success" => WorkflowScenario.Success,
            "inventory-failure" => WorkflowScenario.InventoryFailure,
            "payment-failure" => WorkflowScenario.PaymentFailure,
            "payment-decline" => WorkflowScenario.PaymentDecline,
            "notification-signalr" => WorkflowScenario.NotificationSignalR,
            "seller-authorization" => WorkflowScenario.SellerAuthorization,
            "shipping-failure" => WorkflowScenario.ShippingFailure,
            "customer-cancellation" => WorkflowScenario.CustomerCancellation,
            _ => throw new ArgumentException(
                "--scenario must be one of: all, basket-checkout, success, inventory-failure, payment-failure, payment-decline, notification-signalr, seller-authorization, shipping-failure, customer-cancellation.")
        };
    }
}
