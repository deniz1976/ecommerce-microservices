namespace ECommerce.ContractTests;

public sealed class InventoryStockMovementArchitectureTests
{
    [Fact]
    public void Movement_reader_is_product_scoped_bounded_and_tracking_free()
    {
        string reader = ReadRepositoryFile(
            "src", "Services", "Inventory", "ECommerce.Inventory.Infrastructure",
            "Persistence", "StockMovementReader.cs");

        Assert.Contains(".AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("movement.ProductId == criteria.ProductId", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("PagedResult<StockMovementResponse>", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("PagedResult<StockMovement>", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void Movement_endpoint_is_admin_only_and_has_protected_gateway_route()
    {
        string controller = ReadRepositoryFile(
            "src", "Services", "Inventory", "ECommerce.Inventory.Api",
            "Inventory", "InventoryController.cs");
        string gateway = ReadRepositoryFile(
            "src", "ApiGateways", "ECommerce.ApiGateway", "ocelot.json");

        Assert.Contains("SearchStockMovements", controller, StringComparison.Ordinal);
        Assert.Contains("AuthorizationPolicies.Admin", controller, StringComparison.Ordinal);
        Assert.Contains("/gateway/inventory/items/{productId}/movements", gateway, StringComparison.Ordinal);
    }

    [Fact]
    public void Audit_capture_occurs_inside_inventory_save_changes()
    {
        string dbContext = ReadRepositoryFile(
            "src", "Services", "Inventory", "ECommerce.Inventory.Infrastructure",
            "Persistence", "InventoryDbContext.cs");
        string itemConfiguration = ReadRepositoryFile(
            "src", "Services", "Inventory", "ECommerce.Inventory.Infrastructure",
            "Persistence", "Configurations", "InventoryItemConfiguration.cs");
        string movementConfiguration = ReadRepositoryFile(
            "src", "Services", "Inventory", "ECommerce.Inventory.Infrastructure",
            "Persistence", "Configurations", "StockMovementConfiguration.cs");

        Assert.Contains("CapturePendingStockMovements();", dbContext, StringComparison.Ordinal);
        Assert.Contains("base.SaveChangesAsync(cancellationToken)", dbContext, StringComparison.Ordinal);
        Assert.Contains(".IsConcurrencyToken()", itemConfiguration, StringComparison.Ordinal);
        Assert.Contains("reservation_id IS NOT NULL", movementConfiguration, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null &&
            !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        string root = directory?.FullName
            ?? throw new DirectoryNotFoundException("Repository root was not found.");
        return File.ReadAllText(Path.Combine([root, .. segments]));
    }
}
