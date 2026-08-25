namespace ECommerce.ContractTests;

public sealed class FrontendAdminStoreWorkspaceTests
{
    [Fact]
    public void AdminStoreClientUsesAuthenticatedBoundedServerQuery()
    {
        string api = ReadFrontendFile("lib", "api", "catalog.ts");

        Assert.Contains("getManagedCatalogStores(", api, StringComparison.Ordinal);
        Assert.Contains("/gateway/catalog/stores/manage?", api, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber", api, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize", api, StringComparison.Ordinal);
        Assert.Contains("ownerUserId", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true, signal", api, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicStoreDetailUsesPublicStoreAndBoundedProductQueries()
    {
        string route = ReadFrontendFile("app", "stores", "[id]", "page.tsx");
        string detail = ReadFrontendFile("components", "customer", "store-detail.tsx");
        string api = ReadFrontendFile("lib", "api", "catalog.ts");

        Assert.Contains("<StoreDetail", route, StringComparison.Ordinal);
        Assert.Contains("getCatalogStore(params.id)", detail, StringComparison.Ordinal);
        Assert.Contains("getPublicCatalogProducts", detail, StringComparison.Ordinal);
        Assert.Contains("storeId: params.id", detail, StringComparison.Ordinal);
        Assert.Contains("/gateway/catalog/stores/${storeId}", api, StringComparison.Ordinal);
        Assert.Contains("parameters.set(\"storeId\"", api, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminStoreRouteIsGuardedAndUsesServerPagination()
    {
        string route = ReadFrontendFile("app", "admin", "stores", "page.tsx");
        string workspace = ReadFrontendFile(
            "components", "admin", "admin-store-management-page.tsx");
        string dashboard = ReadFrontendFile("components", "admin", "admin-dashboard.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getManagedCatalogStores(", workspace, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", workspace, StringComparison.Ordinal);
        Assert.Contains("isOptionalGuid", workspace, StringComparison.Ordinal);
        Assert.Contains("<ReferencePagination", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain(".items.slice(", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("createCatalogStore", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("updateCatalogStore", workspace, StringComparison.Ordinal);
        Assert.Contains("href=\"/admin/stores\"", dashboard, StringComparison.Ordinal);
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
