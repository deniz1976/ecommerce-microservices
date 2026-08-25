namespace ECommerce.ContractTests;

public sealed class IdentityUserResponsibilityArchitectureTests
{
    [Fact]
    public void UserProvisioningIsSplitByIdentitySource()
    {
        string directory = RepositoryPath(
            "src", "Services", "Identity",
            "ECommerce.Identity.Application", "Users");

        Assert.False(File.Exists(Path.Combine(directory, "UserService.cs")));
        string registration = File.ReadAllText(
            Path.Combine(directory, "UserRegistrationService.cs"));
        string external = File.ReadAllText(
            Path.Combine(directory, "ExternalUserProvisioningService.cs"));

        Assert.Contains("RegisterAsync", registration, StringComparison.Ordinal);
        Assert.Contains("IPasswordHashService", registration, StringComparison.Ordinal);
        Assert.DoesNotContain("GetOrCreateExternalAsync", registration, StringComparison.Ordinal);
        Assert.DoesNotContain("LinkExternalIdentity", registration, StringComparison.Ordinal);

        Assert.Contains("GetOrCreateExternalAsync", external, StringComparison.Ordinal);
        Assert.DoesNotContain("LinkExternalIdentity", external, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterAsync", external, StringComparison.Ordinal);
        Assert.DoesNotContain("IPasswordHashService", external, StringComparison.Ordinal);
    }

    private static string RepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
