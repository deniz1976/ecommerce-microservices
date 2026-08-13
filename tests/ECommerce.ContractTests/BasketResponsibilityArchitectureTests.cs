using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Application.Commands.AddBasketItem;
using ECommerce.Basket.Application.Commands.ClearBasket;
using ECommerce.Basket.Application.Commands.RemoveBasketItem;

namespace ECommerce.ContractTests;

public sealed class BasketResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(AddBasketItemCommandHandler), typeof(BasketItemAdditionService))]
    [InlineData(typeof(RemoveBasketItemCommandHandler), typeof(BasketItemRemovalService))]
    [InlineData(typeof(ClearBasketCommandHandler), typeof(BasketClearService))]
    public void MutationHandlersDependOnTheirFocusedService(Type handlerType, Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public void BasketApplicationOperationsAreSplitByResponsibility()
    {
        string basketDirectory = RepositoryPath(
            "src", "Services", "Basket", "ECommerce.Basket.Application", "Baskets");

        Assert.False(File.Exists(Path.Combine(basketDirectory, "BasketService.cs")));

        string queryService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketQueryService.cs"));
        string additionService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketItemAdditionService.cs"));
        string removalService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketItemRemovalService.cs"));
        string checkoutService = File.ReadAllText(
            Path.Combine(basketDirectory, "BasketCheckoutService.cs"));

        Assert.Contains("IActiveBasketStore", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductCatalogReader", queryService, StringComparison.Ordinal);
        Assert.DoesNotContain("IUnitOfWork", queryService, StringComparison.Ordinal);

        Assert.Contains("IProductCatalogReader", additionService, StringComparison.Ordinal);
        Assert.DoesNotContain("IProductCatalogReader", removalService, StringComparison.Ordinal);
        Assert.DoesNotContain("IRepository<", additionService, StringComparison.Ordinal);
        Assert.DoesNotContain("ICheckoutPublisher", additionService, StringComparison.Ordinal);

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
