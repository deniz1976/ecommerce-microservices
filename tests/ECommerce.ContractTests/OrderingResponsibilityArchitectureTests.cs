namespace ECommerce.ContractTests;

public sealed class OrderingResponsibilityArchitectureTests
{
    [Fact]
    public void OrderDetailReadIsSeparatedFromCreationOrchestration()
    {
        string directory = RepositoryPath(
            "src", "Services", "Ordering",
            "ECommerce.Ordering.Application", "Orders");
        string creation = File.ReadAllText(Path.Combine(directory, "OrderService.cs"));
        string detail = File.ReadAllText(Path.Combine(directory, "OrderDetailQueryService.cs"));

        Assert.Contains("CreateAsync", creation, StringComparison.Ordinal);
        Assert.Contains("CreateFromCheckoutAsync", creation, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Task<Result<OrderResponse>> GetByIdAsync",
            creation,
            StringComparison.Ordinal);

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
