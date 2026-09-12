namespace ECommerce.ContractTests;

public sealed class FrontendAdminShipmentWorkspaceTests
{
    [Fact]
    public void AdminShipmentClientUsesAuthenticatedBoundedServerQuery()
    {
        string api = ReadFrontendFile("lib", "api", "shipping.ts");

        Assert.Contains("getManagedShipments(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/shipments/manage?", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("createdFrom", api, StringComparison.Ordinal);
        Assert.Contains("createdTo", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal },", api, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminShipmentRouteIsGuardedAndUsesSafeServerPagination()
    {
        string route = ReadFrontendFile("app", "admin", "shipments", "page.tsx");
        string workspace = ReadFrontendFile(
            "components", "admin", "admin-shipment-management-page.tsx");
        string filters = ReadFrontendFile("lib", "admin", "filters.ts");
        string dashboard = ReadFrontendFile("components", "admin", "admin-dashboard.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getManagedShipments(", workspace, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("isOptionalGuid", workspace, StringComparison.Ordinal);
        Assert.Contains("isValidUtcRange", workspace, StringComparison.Ordinal);
        Assert.Contains("toOptionalUtcIso", workspace, StringComparison.Ordinal);
        Assert.Contains("toISOString()", filters, StringComparison.Ordinal);
        Assert.Contains("<DataTable", workspace, StringComparison.Ordinal);
        Assert.Contains("onPageChange=", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("address", workspace, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"/admin/shipments\"", dashboard, StringComparison.Ordinal);
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
