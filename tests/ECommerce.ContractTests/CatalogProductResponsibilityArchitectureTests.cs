namespace ECommerce.ContractTests;

public sealed class CatalogProductResponsibilityArchitectureTests
{
    [Fact]
    public void ProductApplicationOperationsAreSplitByCqrsResponsibility()
    {
        string productDirectory = RepositoryPath(
            "src",
            "Services",
            "Catalog",
            "ECommerce.Catalog.Application",
            "Products");

        Assert.False(File.Exists(Path.Combine(productDirectory, "ProductService.cs")));

        string queryService = File.ReadAllText(
            Path.Combine(productDirectory, "ProductQueryService.cs"));
        string managementService = File.ReadAllText(
            Path.Combine(productDirectory, "ProductManagementService.cs"));

        Assert.Contains("IProductSearchReader", queryService, StringComparison.Ordinal);
        Assert.Contains("SearchPublicAsync", queryService, StringComparison.Ordinal);
        Assert.Contains("GetManagedByIdAsync", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IUnitOfWork", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateAsync", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateAsync", queryService, StringComparison.Ordinal);

        Assert.Contains("IUnitOfWork", managementService, StringComparison.Ordinal);
        Assert.Contains("IProductReferenceValidator", managementService, StringComparison.Ordinal);
        Assert.Contains("CreateAsync", managementService, StringComparison.Ordinal);
        Assert.Contains("UpdateAsync", managementService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductSearchReader", managementService, StringComparison.Ordinal);
        Assert.DoesNotContain("SearchPublicAsync", managementService, StringComparison.Ordinal);
        Assert.DoesNotContain("GetManagedByIdAsync", managementService, StringComparison.Ordinal);
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
