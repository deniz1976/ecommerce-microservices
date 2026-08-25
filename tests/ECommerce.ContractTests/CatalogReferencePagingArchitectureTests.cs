namespace ECommerce.ContractTests;

public sealed class CatalogReferencePagingArchitectureTests
{
    [Fact]
    public void ManagedReferenceReaderProjectsPagedDtosWithoutLoadingAggregates()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Infrastructure",
            "Persistence",
            "CatalogReferenceReader.cs"));
        string managedSection = reader[reader.IndexOf(
            "Task<PagedResult<ManagedCatalogCategoryResponse>>",
            StringComparison.Ordinal)..];

        Assert.Contains("AsNoTracking()", managedSection, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", managedSection, StringComparison.Ordinal);
        Assert.Contains("Select(category =>", managedSection, StringComparison.Ordinal);
        Assert.Contains("Select(brand =>", managedSection, StringComparison.Ordinal);
        Assert.Contains(".Skip(", managedSection, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", managedSection, StringComparison.Ordinal);
        Assert.Contains("EF.Functions.ILike", managedSection, StringComparison.Ordinal);
        Assert.Contains("ThenBy(category => category.Id)", managedSection, StringComparison.Ordinal);
        Assert.Contains("ThenBy(brand => brand.Id)", managedSection, StringComparison.Ordinal);
        Assert.True(
            managedSection.IndexOf("categories = ApplyCategoryOrdering", StringComparison.Ordinal) <
            managedSection.IndexOf("IQueryable<ManagedCatalogCategoryResponse> projection", StringComparison.Ordinal));
        Assert.True(
            managedSection.IndexOf("brands = ApplyBrandOrdering", StringComparison.Ordinal) <
            managedSection.IndexOf("IQueryable<ManagedCatalogBrandResponse> projection", StringComparison.Ordinal));
        Assert.DoesNotContain(".Include(", managedSection, StringComparison.Ordinal);
    }

    [Fact]
    public void ManagedReferenceQueriesReturnBoundedPagedProjections()
    {
        string limits = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Application",
            "References",
            "CatalogReferenceQueryLimits.cs"));
        string readerContract = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Application",
            "References",
            "ICatalogReferenceManagementReader.cs"));

        Assert.Contains("MaxPageSize = 100", limits, StringComparison.Ordinal);
        Assert.Contains("MaxSearchLength = 200", limits, StringComparison.Ordinal);
        Assert.Contains("PagedResult<ManagedCatalogCategoryResponse>", readerContract, StringComparison.Ordinal);
        Assert.Contains("PagedResult<ManagedCatalogBrandResponse>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("IReadOnlyCollection<Category>", readerContract, StringComparison.Ordinal);
        Assert.DoesNotContain("IReadOnlyCollection<Brand>", readerContract, StringComparison.Ordinal);
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
