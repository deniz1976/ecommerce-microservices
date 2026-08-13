namespace ECommerce.ContractTests;

public sealed class OrderingSagaTimeoutArchitectureTests
{
    [Fact]
    public void Timeout_dispatch_is_durable_bounded_and_tracking_free()
    {
        string root = FindRepositoryRoot();
        string reader = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Services",
            "OrderingSaga",
            "ECommerce.OrderingSaga.Infrastructure",
            "Persistence",
            "OrderWorkflowTimeoutReader.cs"));
        string service = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Services",
            "OrderingSaga",
            "ECommerce.OrderingSaga.Application",
            "Workflows",
            "OrderWorkflowTimeoutService.cs"));

        Assert.Contains(".AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(batchSize)", reader, StringComparison.Ordinal);
        Assert.Contains("workflow.TimeoutHandledAt == null", reader, StringComparison.Ordinal);
        Assert.True(
            service.IndexOf("publisher.CancelOrderAsync", StringComparison.Ordinal) <
            service.IndexOf("unitOfWork.SaveChangesAsync", StringComparison.Ordinal));
    }

    [Fact]
    public void Timeout_configuration_is_explicit_and_bounded()
    {
        string root = FindRepositoryRoot();
        string configuration = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Services",
            "OrderingSaga",
            "ECommerce.OrderingSaga.Infrastructure",
            "DependencyInjection.cs"));
        string compose = File.ReadAllText(Path.Combine(root, "docker-compose.yml"));

        Assert.Contains("InventorySeconds is < 5 or > 3600", configuration, StringComparison.Ordinal);
        Assert.Contains("PaymentSeconds is < 5 or > 3600", configuration, StringComparison.Ordinal);
        Assert.Contains("ShippingSeconds is < 5 or > 3600", configuration, StringComparison.Ordinal);
        Assert.Contains("ORDER_WORKFLOW_INVENTORY_TIMEOUT_SECONDS", compose, StringComparison.Ordinal);
        Assert.Contains("ORDER_WORKFLOW_PAYMENT_TIMEOUT_SECONDS", compose, StringComparison.Ordinal);
        Assert.Contains("ORDER_WORKFLOW_SHIPPING_TIMEOUT_SECONDS", compose, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null &&
            !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                "Repository root containing ECommerce.sln was not found.");
    }
}
