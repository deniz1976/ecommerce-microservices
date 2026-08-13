namespace ECommerce.ContractTests;

public sealed class BasketResponsibilityArchitectureTests
{
    [Fact]
    public void BasketApplicationOperationsAreSplitByResponsibility()
    {
        string basketDirectory = RepositoryPath(
            "src", "Services", "Basket", "ECommerce.Basket.Application", "Baskets");

        Assert.False(File.Exists(Path.Combine(basketDirectory, "BasketService.cs")));

        string queryService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketQueryService.cs"));
        string mutationService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketMutationService.cs"));
        string checkoutService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketCheckoutService.cs"));

        Assert.Contains("IActiveBasketStore", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductCatalogReader", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IUnitOfWork", queryService, StringComparison.Ordinal);

        Assert.Contains("IProductCatalogReader", mutationService, StringComparison.Ordinal);
        Assert.DoesNotContain("IRepository<", mutationService, StringComparison.Ordinal);
        Assert.DoesNotContain("ICheckoutPublisher", mutationService, StringComparison.Ordinal);

        Assert.Contains("ICheckoutPublisher", checkoutService, StringComparison.Ordinal);
        Assert.Contains("IUnitOfWork", checkoutService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductCatalogReader", checkoutService, StringComparison.Ordinal);
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
