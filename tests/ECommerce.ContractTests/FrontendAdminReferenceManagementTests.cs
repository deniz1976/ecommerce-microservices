namespace ECommerce.ContractTests;

public sealed class FrontendAdminReferenceManagementTests
{
    [Fact]
    public void CategoriesAndBrandsHaveIndependentGuardedRoutes()
    {
        string categoryRoute = File.ReadAllText(GetFrontendPath(
            "app",
            "admin",
            "categories",
            "page.tsx"));
        string brandRoute = File.ReadAllText(GetFrontendPath(
            "app",
            "admin",
            "brands",
            "page.tsx"));

        Assert.Contains("<AdminRouteGuard>", categoryRoute, StringComparison.Ordinal);
        Assert.Contains("<AdminCategoryManagementPage />", categoryRoute, StringComparison.Ordinal);
        Assert.Contains("<AdminRouteGuard>", brandRoute, StringComparison.Ordinal);
        Assert.Contains("<AdminBrandManagementPage />", brandRoute, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferencePagesProvideSearchStatusSortAndPaginationControls()
    {
        string dataTable = File.ReadAllText(GetFrontendPath(
            "components",
            "patterns",
            "data-table.tsx"));
        string filterBar = File.ReadAllText(GetFrontendPath(
            "components",
            "patterns",
            "filter-bar.tsx"));
        string categories = File.ReadAllText(GetFrontendPath(
            "components",
            "admin",
            "admin-category-management-page.tsx"));

        Assert.Contains("DataTableSort", dataTable, StringComparison.Ordinal);
        Assert.Contains("aria-sort", dataTable, StringComparison.Ordinal);
        Assert.Contains("onPageChange(pageNumber - 1)", dataTable, StringComparison.Ordinal);
        Assert.Contains("onPageChange(pageNumber + 1)", dataTable, StringComparison.Ordinal);
        Assert.Contains("search", filterBar, StringComparison.Ordinal);
        Assert.Contains("hasActiveFilters", filterBar, StringComparison.Ordinal);
        Assert.Contains("allReferenceStatuses", categories, StringComparison.Ordinal);
        Assert.Contains("[10, 25, 50]", categories, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferencePagesUseServerPagingAndCancelObsoleteSearches()
    {
        string categories = File.ReadAllText(GetFrontendPath(
            "components",
            "admin",
            "admin-category-management-page.tsx"));
        string brands = File.ReadAllText(GetFrontendPath(
            "components",
            "admin",
            "admin-brand-management-page.tsx"));

        Assert.Contains("useDebouncedValue(search, 300)", categories, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", categories, StringComparison.Ordinal);
        Assert.Contains("pageNumber: page", categories, StringComparison.Ordinal);
        Assert.Contains("<DataTable", categories, StringComparison.Ordinal);
        Assert.Contains("categories.data", categories, StringComparison.Ordinal);
        Assert.DoesNotContain("useMemo", categories, StringComparison.Ordinal);
        Assert.DoesNotContain(".slice(", categories, StringComparison.Ordinal);
        Assert.Contains("editingCategory", categories, StringComparison.Ordinal);
        Assert.Contains("useDebouncedValue(search, 300)", brands, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", brands, StringComparison.Ordinal);
        Assert.Contains("pageNumber: page", brands, StringComparison.Ordinal);
        Assert.Contains("<DataTable", brands, StringComparison.Ordinal);
        Assert.Contains("brands.data", brands, StringComparison.Ordinal);
        Assert.DoesNotContain("useMemo", brands, StringComparison.Ordinal);
        Assert.DoesNotContain(".slice(", brands, StringComparison.Ordinal);
        Assert.Contains("editingBrand", brands, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferenceApiSendsOnlyBoundedServerQueryParameters()
    {
        string catalogApi = File.ReadAllText(GetFrontendPath(
            "lib",
            "api",
            "catalog.ts"));

        Assert.Contains("Promise<PagedResult<ManagedCatalogCategoryReference>>", catalogApi, StringComparison.Ordinal);
        Assert.Contains("Promise<PagedResult<ManagedCatalogBrandReference>>", catalogApi, StringComparison.Ordinal);
        Assert.Contains("pageNumber: String(query.pageNumber)", catalogApi, StringComparison.Ordinal);
        Assert.Contains("pageSize: String(query.pageSize)", catalogApi, StringComparison.Ordinal);
        Assert.Contains("sortDescending: String(query.sortDescending)", catalogApi, StringComparison.Ordinal);
        Assert.Contains("signal?: AbortSignal", catalogApi, StringComparison.Ordinal);
    }

    private static string GetFrontendPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            DirectoryInfo? parent = Directory.GetParent(root);
            root = parent?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, "src", "Frontend", .. segments]);
    }
}
