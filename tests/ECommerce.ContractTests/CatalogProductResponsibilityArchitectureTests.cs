using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.Queries.GetManagedProduct;
using ECommerce.Catalog.Application.Queries.GetProduct;
using ECommerce.Catalog.Application.Queries.SearchManagedProducts;
using ECommerce.Catalog.Application.Queries.SearchProducts;

namespace ECommerce.ContractTests;

public sealed class CatalogProductResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(SearchProductsQueryHandler), typeof(PublicProductQueryService))]
    [InlineData(typeof(GetProductQueryHandler), typeof(PublicProductQueryService))]
    [InlineData(typeof(SearchManagedProductsQueryHandler), typeof(ManagedProductQueryService))]
    [InlineData(typeof(GetManagedProductQueryHandler), typeof(ManagedProductQueryService))]
    public void ProductQueryHandlersDependOnTheirFocusedService(Type handlerType, Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

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

        string publicQueryService = File.ReadAllText(
            Path.Combine(productDirectory, "PublicProductQueryService.cs"));
        string managedQueryService = File.ReadAllText(
            Path.Combine(productDirectory, "ManagedProductQueryService.cs"));
        string managementService = File.ReadAllText(
            Path.Combine(productDirectory, "ProductManagementService.cs"));

        Assert.Contains("IProductSearchReader", publicQueryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductStoreAccessValidator", publicQueryService, StringComparison.Ordinal);
        Assert.Contains("IProductStoreAccessValidator", managedQueryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IUnitOfWork", publicQueryService, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateAsync", publicQueryService, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateAsync", publicQueryService, StringComparison.Ordinal);

        Assert.Contains("IUnitOfWork", managementService, StringComparison.Ordinal);
        Assert.Contains("IProductReferenceValidator", managementService, StringComparison.Ordinal);
        Assert.Contains("CreateAsync", managementService, StringComparison.Ordinal);
        Assert.Contains("UpdateAsync", managementService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductSearchReader", managementService, StringComparison.Ordinal);
        Assert.DoesNotContain("SearchAsync", managementService, StringComparison.Ordinal);
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
