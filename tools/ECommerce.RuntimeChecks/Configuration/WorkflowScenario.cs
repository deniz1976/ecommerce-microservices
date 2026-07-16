namespace ECommerce.RuntimeChecks.Configuration;

public enum WorkflowScenario
{
    All,
    Success,
    InventoryFailure,
    PaymentFailure,
    ShippingFailure
}

public static class WorkflowScenarios
{
    public static WorkflowScenario Parse(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "all" => WorkflowScenario.All,
            "success" => WorkflowScenario.Success,
            "inventory-failure" => WorkflowScenario.InventoryFailure,
            "payment-failure" => WorkflowScenario.PaymentFailure,
            "shipping-failure" => WorkflowScenario.ShippingFailure,
            _ => throw new ArgumentException(
                "--scenario must be one of: all, success, inventory-failure, payment-failure, shipping-failure.")
        };
    }
}
