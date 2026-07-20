using ECommerce.RuntimeChecks.Configuration;

namespace ECommerce.ContractTests;

public sealed class WorkflowScenarioTests
{
    [Theory]
    [InlineData(null, WorkflowScenario.All)]
    [InlineData("all", WorkflowScenario.All)]
    [InlineData("success", WorkflowScenario.Success)]
    [InlineData("inventory-failure", WorkflowScenario.InventoryFailure)]
    [InlineData("payment-failure", WorkflowScenario.PaymentFailure)]
    [InlineData("shipping-failure", WorkflowScenario.ShippingFailure)]
    public void ParseMapsDocumentedScenarioName(string? value, WorkflowScenario expected)
    {
        Assert.Equal(expected, WorkflowScenarios.Parse(value));
    }

    [Fact]
    public void ParseRejectsUnknownScenario()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => WorkflowScenarios.Parse("unknown-failure"));

        Assert.Contains("--scenario", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeTokenRequestsOnlyRequiredPermissions()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "request-runtime-access-token.ps1"));

        Assert.Contains("scope = \"inventory:write customer:act\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Admin", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Auth0ManagementPreflightRequestsOnlyUserUpdatePermission()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "test-auth0-management-access.ps1"));
        string workflow = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            ".github",
            "workflows",
            "runtime-integration.yml"));

        Assert.Contains("scope = \"update:users\"", script, StringComparison.Ordinal);
        Assert.Contains("exactly the update:users scope", script, StringComparison.Ordinal);
        Assert.DoesNotContain("create:users", script, StringComparison.Ordinal);
        Assert.DoesNotContain("delete:users", script, StringComparison.Ordinal);
        Assert.DoesNotContain("update:clients", script, StringComparison.Ordinal);
        Assert.Contains("./scripts/test-auth0-management-access.ps1", workflow, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Repository root containing ECommerce.sln was not found.");
    }
}
