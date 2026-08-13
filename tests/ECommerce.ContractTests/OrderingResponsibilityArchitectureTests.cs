using ECommerce.Ordering.Application.Commands.CreateOrder;
using ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.ContractTests;

public sealed class OrderingResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(CreateOrderCommandHandler), typeof(OrderCreationService))]
    [InlineData(typeof(CreateOrderFromCheckoutCommandHandler), typeof(CheckoutOrderCreationService))]
    public void CreationHandlersDependOnTheirFocusedService(Type handlerType, Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public void OrderDetailReadIsSeparatedFromCreationOrchestration()
    {
        string directory = RepositoryPath(
            "src", "Services", "Ordering",
            "ECommerce.Ordering.Application", "Orders");
        string creation = File.ReadAllText(Path.Combine(directory, "OrderCreationService.cs"));
        string checkout = File.ReadAllText(Path.Combine(directory, "CheckoutOrderCreationService.cs"));
        string detail = File.ReadAllText(Path.Combine(directory, "OrderDetailQueryService.cs"));

        Assert.Contains("CreateAsync", creation, StringComparison.Ordinal);
        Assert.DoesNotContain("BasketCheckedOut", creation, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Task<Result<OrderResponse>> GetByIdAsync",
            creation,
            StringComparison.Ordinal);
        Assert.Contains("BasketCheckedOut", checkout, StringComparison.Ordinal);
        Assert.Contains("CheckoutId", checkout, StringComparison.Ordinal);

        Assert.Contains("GetByIdAsync", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("IUnitOfWork", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("IOrderSubmittedPublisher", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateAsync", detail, StringComparison.Ordinal);
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
