namespace ECommerce.ContractTests;

public sealed class FrontendAdminWorkflowDiagnosticsTests
{
    [Fact]
    public void AdminWorkflowDiagnosticsIsGuardedAndUsesBoundedServerQuery()
    {
        string route = ReadFrontendFile("app", "admin", "workflows", "page.tsx");
        string page = ReadFrontendFile(
            "components", "admin", "admin-workflow-diagnostics-page.tsx");
        string api = ReadFrontendFile("lib", "api", "workflows.ts");
        string shell = ReadFrontendFile("components", "admin", "admin-shell.tsx");

        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("getWorkflowDiagnostics(", page, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", page, StringComparison.Ordinal);
        Assert.Contains("<DataTable", page, StringComparison.Ordinal);
        Assert.Contains("onPageChange=", page, StringComparison.Ordinal);
        Assert.Contains("/gateway/workflows/diagnostics?", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true, signal", api, StringComparison.Ordinal);
        Assert.Contains("href: \"/admin/workflows\"", shell, StringComparison.Ordinal);
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
