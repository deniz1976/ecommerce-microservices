namespace ECommerce.ContractTests;

public sealed class FrontendAdminInventoryWorkspaceTests
{
    [Fact]
    public void AdminInventoryClientUsesAuthenticatedBoundedServerQuery()
    {
        string api = ReadFrontendFile("lib", "api", "inventory.ts");

        Assert.Contains("getManagedInventory(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/inventory/items/manage?", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("maximumAvailableQuantity", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal },", api, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminInventoryRouteIsGuardedAndUsesServerPagination()
    {
        string route = ReadFrontendFile("app", "admin", "inventory", "page.tsx");
        string workspace = ReadFrontendFile(
            "components", "admin", "admin-inventory-management-page.tsx");
        string dashboard = ReadFrontendFile("components", "admin", "admin-dashboard.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getManagedInventory(", workspace, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("guidPattern", workspace, StringComparison.Ordinal);
        Assert.Contains("maximumValid", workspace, StringComparison.Ordinal);
        Assert.Contains("<ReferencePagination", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.Contains("href=\"/admin/inventory\"", dashboard, StringComparison.Ordinal);
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
