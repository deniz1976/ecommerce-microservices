namespace ECommerce.ContractTests;

public sealed class OrderListPagingArchitectureTests
{
    [Fact]
    public void OrderListsUseBoundedDatabaseProjectionWithoutAggregateIncludes()
    {
        string reader = File.ReadAllText(GetRepositoryPath(
            "src",
            "Services",
            "Ordering",
            "ECommerce.Ordering.Infrastructure",
            "Persistence",
            "OrderReader.cs"));

        Assert.Contains("AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("LongCountAsync", reader, StringComparison.Ordinal);
        Assert.Contains(".Skip(", reader, StringComparison.Ordinal);
        Assert.Contains(".Take(pageSize)", reader, StringComparison.Ordinal);
        Assert.Contains("Select(order => new OrderSummaryResponse", reader, StringComparison.Ordinal);
        Assert.Contains("ThenBy(order => order.Id)", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Include(", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomerFrontendConsumesPagedOrderSummaries()
    {
        string api = File.ReadAllText(GetRepositoryPath(
            "src",
            "Frontend",
            "lib",
            "api",
            "orders.ts"));
        string history = File.ReadAllText(GetRepositoryPath(
            "src",
            "Frontend",
            "components",
            "customer",
            "order-history.tsx"));

        Assert.Contains("Promise<PagedResult<OrderSummary>>", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(pageNumber)", api, StringComparison.Ordinal);
        Assert.Contains("state.data.items.map", history, StringComparison.Ordinal);
        Assert.DoesNotContain("Promise<Order[]>", api, StringComparison.Ordinal);
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
