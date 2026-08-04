namespace ECommerce.ContractTests;

public sealed class ManagedShipmentPagingArchitectureTests
{
    [Fact]
    public void ShipmentReaderUsesBoundedNoTrackingSafeProjection()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Shipping",
            "ECommerce.Shipping.Infrastructure",
            "Persistence",
            "ShipmentQueryReader.cs"));

        Assert.Contains("AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", reader, StringComparison.Ordinal);
        Assert.Contains("Select(shipment =>", reader, StringComparison.Ordinal);
        Assert.Contains(".Skip(", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("ThenBy(item => item.Id)", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("FailureReason", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("AddressLine", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Include(", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void ManagedShipmentQueryIsBoundedAndDoesNotExposeAggregates()
    {
        string limits = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Shipping",
            "ECommerce.Shipping.Application",
            "Shipments",
            "ManagedShipmentQueryLimits.cs"));
        string readerContract = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Shipping",
            "ECommerce.Shipping.Application",
            "Shipments",
            "IShipmentQueryReader.cs"));

        Assert.Contains("MaxPageSize = 100", limits, StringComparison.Ordinal);
        Assert.Contains("PagedResult<ShipmentResponse>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("PagedResult<Shipment>", readerContract, StringComparison.Ordinal);
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
