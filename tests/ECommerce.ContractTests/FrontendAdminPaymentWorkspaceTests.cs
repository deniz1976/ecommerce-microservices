namespace ECommerce.ContractTests;

public sealed class FrontendAdminPaymentWorkspaceTests
{
    [Fact]
    public void AdminPaymentClientUsesAuthenticatedBoundedServerQuery()
    {
        string api = ReadFrontendFile("lib", "api", "payments.ts");

        Assert.Contains("getManagedPayments(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/payments/manage?", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("createdFrom", api, StringComparison.Ordinal);
        Assert.Contains("createdTo", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal },", api, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminPaymentRouteIsGuardedAndUsesSafeServerPagination()
    {
        string route = ReadFrontendFile("app", "admin", "payments", "page.tsx");
        string workspace = ReadFrontendFile(
            "components", "admin", "admin-payment-management-page.tsx");
        string dashboard = ReadFrontendFile("components", "admin", "admin-dashboard.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getManagedPayments(", workspace, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("isOptionalGuid", workspace, StringComparison.Ordinal);
        Assert.Contains("dateRangeValid", workspace, StringComparison.Ordinal);
        Assert.Contains("toOptionalUtcIso", workspace, StringComparison.Ordinal);
        Assert.Contains("<ReferencePagination", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("transactions", workspace, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"/admin/payments\"", dashboard, StringComparison.Ordinal);
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
