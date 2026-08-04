namespace ECommerce.ContractTests;

public sealed class MigrationEnvironmentScriptTests
{
    [Fact]
    public void MigrationLauncherRequiresAnExplicitSupportedEnvironment()
    {
        string script = ReadRepositoryFile("scripts", "apply-migrations.ps1");

        Assert.Contains("[Parameter(Mandatory = $true)]", script, StringComparison.Ordinal);
        Assert.Contains("[ValidateSet(\"dev\", \"staging\")]", script, StringComparison.Ordinal);
        Assert.DoesNotContain("[string]$Environment =", script, StringComparison.Ordinal);
    }

    [Fact]
    public void MigrationLauncherDelegatesThroughInfisicalWithoutReadingSecrets()
    {
        string script = ReadRepositoryFile("scripts", "apply-migrations.ps1");

        Assert.Contains("run-with-secrets.ps1", script, StringComparison.Ordinal);
        Assert.Contains("run-migrations.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-Environment $Environment", script, StringComparison.Ordinal);
        Assert.Contains("-Service", script, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionStrings__", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Get-Content", script, StringComparison.Ordinal);
    }

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
