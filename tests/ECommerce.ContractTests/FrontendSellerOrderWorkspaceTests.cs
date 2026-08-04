namespace ECommerce.ContractTests;

public sealed class FrontendSellerOrderWorkspaceTests
{
    [Fact]
    public void SellerOrderClientUsesAuthenticatedServerPagingAndCancellation()
    {
        string api = ReadFrontendFile("lib", "api", "orders.ts");

        Assert.Contains("getSellerOrders(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/orders/store/${storeId}", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("sortDescending: String(query.sortDescending", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal,", api, StringComparison.Ordinal);
    }

    [Fact]
    public void SellerOrderPageIsRoleGuardedAndDoesNotClientPageResults()
    {
        string page = ReadFrontendFile("app", "seller", "orders", "page.tsx");
        string guard = ReadFrontendFile("components", "seller", "seller-route-guard.tsx");
        string workspace = ReadFrontendFile(
            "components", "seller", "seller-order-management-page.tsx");

        Assert.Contains("<SellerRouteGuard>", page, StringComparison.Ordinal);
        Assert.Contains("role === \"Seller\" || role === \"Admin\"", guard, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("getSellerOrders(", workspace, StringComparison.Ordinal);
        Assert.Contains("setPageSize", workspace, StringComparison.Ordinal);
        Assert.Contains("setStatus", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("/orders/${order.orderId}", workspace, StringComparison.Ordinal);
    }

    [Fact]
    public void SellerOrderTypesAndWorkspaceOmitCustomerPii()
    {
        string types = ReadFrontendFile("types", "index.ts");
        int start = types.IndexOf("export interface SellerOrderSummary", StringComparison.Ordinal);
        int end = types.IndexOf("export type PaymentStatus", start, StringComparison.Ordinal);
        string sellerOrderTypes = types[start..end];
        string workspace = ReadFrontendFile(
            "components", "seller", "seller-order-management-page.tsx");

        Assert.DoesNotContain("customerId", sellerOrderTypes, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("address", sellerOrderTypes, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("recipient", sellerOrderTypes, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("customerId", workspace, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("address", workspace, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("recipient", workspace, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FrontendRunnerStartsNextFromTheFrontendWorkingDirectory()
    {
        string script = ReadRepositoryFile("scripts", "run-frontend.ps1");

        Assert.Contains("Push-Location $frontendRoot", script, StringComparison.Ordinal);
        Assert.Contains("finally", script, StringComparison.Ordinal);
        Assert.Contains("Pop-Location", script, StringComparison.Ordinal);
        Assert.DoesNotContain("--prefix", script, StringComparison.Ordinal);
    }

    private static string ReadFrontendFile(params string[] segments)
    {
        return ReadRepositoryFile("src", "Frontend", segments);
    }

    private static string ReadRepositoryFile(
        string firstSegment,
        string secondSegment,
        params string[] remainingSegments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return File.ReadAllText(
            Path.Combine([root, firstSegment, secondSegment, .. remainingSegments]));
    }
}
