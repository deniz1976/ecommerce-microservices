namespace ECommerce.ContractTests;

public sealed class ManagedStorePagingArchitectureTests
{
    [Fact]
    public void ManagedStoreReaderUsesBoundedNoTrackingProjection()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Infrastructure",
            "Persistence",
            "StoreReader.cs"));
        string managedSection = reader[reader.IndexOf(
            "Task<PagedResult<ManagedStoreResponse>>",
            StringComparison.Ordinal)..];

        Assert.Contains("AsNoTracking()", managedSection, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", managedSection, StringComparison.Ordinal);
        Assert.Contains("Select(store =>", managedSection, StringComparison.Ordinal);
        Assert.Contains(".Skip(", managedSection, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", managedSection, StringComparison.Ordinal);
        Assert.Contains("EF.Functions.ILike", managedSection, StringComparison.Ordinal);
        Assert.Contains("ThenBy(item => item.Id)", managedSection, StringComparison.Ordinal);
        Assert.DoesNotContain(".Include(", managedSection, StringComparison.Ordinal);
    }

    [Fact]
    public void ManagedStoreQueryIsBoundedAndDoesNotExposeEntities()
    {
        string limits = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Application",
            "Stores",
            "ManagedStoreQueryLimits.cs"));
        string readerContract = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Application",
            "Stores",
            "IManagedStoreReader.cs"));

        Assert.Contains("MaxPageSize = 100", limits, StringComparison.Ordinal);
        Assert.Contains("MaxSearchLength = 200", limits, StringComparison.Ordinal);
        Assert.Contains("PagedResult<ManagedStoreResponse>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("IReadOnlyCollection<Store>", readerContract, StringComparison.Ordinal);
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
