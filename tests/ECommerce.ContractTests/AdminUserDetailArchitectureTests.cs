namespace ECommerce.ContractTests;

public sealed class AdminUserDetailArchitectureTests
{
    [Fact]
    public void AdminUserDetailUsesSafeNoTrackingProjection()
    {
        string controller = ReadRepositoryFile("src", "Services", "Identity", "ECommerce.Identity.Api", "Users", "UsersController.cs");
        string handler = ReadRepositoryFile("src", "Services", "Identity", "ECommerce.Identity.Application", "Queries", "GetUserById", "GetUserByIdQueryHandler.cs");
        string reader = ReadRepositoryFile("src", "Services", "Identity", "ECommerce.Identity.Infrastructure", "Persistence", "AdminUserReader.cs");
        string response = ReadRepositoryFile("src", "Services", "Identity", "ECommerce.Identity.Application", "AdminUsers", "AdminUserResponse.cs");

        Assert.Contains("Result<AdminUserResponse>", controller, StringComparison.Ordinal);
        Assert.Contains("AdminUserService", handler, StringComparison.Ordinal);
        Assert.Contains("AsNoTracking()", reader, StringComparison.Ordinal);
        Assert.Contains("SingleOrDefaultAsync(cancellationToken)", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", response, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("External", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AdminUserDetailFrontendIsGuardedAndAbortable()
    {
        string api = ReadFrontendFile("lib", "api", "admin-users.ts");
        string route = ReadFrontendFile("app", "admin", "users", "[id]", "page.tsx");
        string detail = ReadFrontendFile("components", "admin", "admin-user-detail-page.tsx");
        string list = ReadFrontendFile("components", "admin", "admin-user-workspace.tsx");

        Assert.Contains("getAdminUser(", api, StringComparison.Ordinal);
        Assert.Contains("`/gateway/users/${userId}`", api, StringComparison.Ordinal);
        Assert.Contains("authenticated: true", api, StringComparison.Ordinal);
        Assert.Contains("signal,", api, StringComparison.Ordinal);
        Assert.Contains("<AdminRouteGuard>", route, StringComparison.Ordinal);
        Assert.Contains("new AbortController()", detail, StringComparison.Ordinal);
        Assert.Contains("error.status === 404", detail, StringComparison.Ordinal);
        Assert.Contains("href={`/admin/users/${user.id}`}", list, StringComparison.Ordinal);
        Assert.DoesNotContain("externalSubject", detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", detail, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadFrontendFile(params string[] segments) =>
        ReadRepositoryFile(["src", "Frontend", .. segments]);

    private static string ReadRepositoryFile(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return File.ReadAllText(Path.Combine([root, .. segments]));
    }
}
