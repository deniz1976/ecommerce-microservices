namespace ECommerce.ContractTests;

public sealed class FrontendAdminOrderWorkspaceTests
{
    [Fact]
    public void AdminOrderClientUsesAuthenticatedBoundedServerQuery()
    {
        string api = ReadFrontendFile("lib", "api", "orders.ts");

        Assert.Contains("getManagedOrders(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/orders/manage?", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("customerId", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal,", api, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminOrderRouteIsGuardedAndUsesServerPagination()
    {
        string route = ReadFrontendFile("app", "admin", "orders", "page.tsx");
        string workspace = ReadFrontendFile(
            "components", "admin", "admin-order-management-page.tsx");
        string shell = ReadFrontendFile("components", "admin", "admin-shell.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getManagedOrders(", workspace, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("isOptionalGuid", workspace, StringComparison.Ordinal);
        Assert.Contains("<DataTable", workspace, StringComparison.Ordinal);
        Assert.Contains("onPageChange=", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.Contains("href: \"/admin/orders\"", shell, StringComparison.Ordinal);
    }

    private static string ReadFrontendFile(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return File.ReadAllText(Path.Combine([root, "src", "Frontend", .. segments]));
    }
}
