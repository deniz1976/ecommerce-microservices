namespace ECommerce.ContractTests;

public sealed class InventoryResponsibilityArchitectureTests
{
    [Fact]
    public void InventoryCommandsAreSplitByResponsibility()
    {
        string directory = RepositoryPath(
            "src", "Services", "Inventory",
            "ECommerce.Inventory.Application", "Inventory");

        Assert.False(File.Exists(Path.Combine(directory, "InventoryService.cs")));
        string reservation = File.ReadAllText(
            Path.Combine(directory, "InventoryReservationService.cs"));
        string management = File.ReadAllText(
            Path.Combine(directory, "InventoryManagementService.cs"));

        Assert.Contains("ReserveAsync", reservation, StringComparison.Ordinal);
        Assert.Contains("ReleaseAsync", reservation, StringComparison.Ordinal);
        Assert.Contains("IStockReservationIdentityReader", reservation, StringComparison.Ordinal);
        Assert.DoesNotContain("UpsertAsync", reservation, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductInventoryAccessAuthorizer", reservation, StringComparison.Ordinal);

        Assert.Contains("UpsertAsync", management, StringComparison.Ordinal);
        Assert.Contains("IProductInventoryAccessAuthorizer", management, StringComparison.Ordinal);
        Assert.DoesNotContain("ReserveAsync", management, StringComparison.Ordinal);
        Assert.DoesNotContain("StockReservation", management, StringComparison.Ordinal);
    }

    private static string RepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
