namespace ECommerce.ContractTests;

public sealed class ManagedInventoryPagingArchitectureTests
{
    [Fact]
    public void InventoryReaderUsesBoundedNoTrackingProjection()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Inventory",
            "ECommerce.Inventory.Infrastructure",
            "Persistence",
            "InventoryQueryReader.cs"));

        Assert.Contains("AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", reader, StringComparison.Ordinal);
        Assert.Contains("Select(item =>", reader, StringComparison.Ordinal);
        Assert.Contains(".Skip(", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("ThenBy(item => item.ProductId)", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Include(", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void ManagedInventoryQueryIsBoundedAndDoesNotExposeEntities()
    {
        string limits = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Inventory",
            "ECommerce.Inventory.Application",
            "Inventory",
            "ManagedInventoryQueryLimits.cs"));
        string readerContract = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Inventory",
            "ECommerce.Inventory.Application",
            "Inventory",
            "IInventoryQueryReader.cs"));

        Assert.Contains("MaxPageSize = 100", limits, StringComparison.Ordinal);
        Assert.Contains("PagedResult<InventoryItemResponse>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("PagedResult<InventoryItem>", readerContract, StringComparison.Ordinal);
    }

    private static string GetRepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            DirectoryInfo? parent = Directory.GetParent(root);
            root = parent?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
