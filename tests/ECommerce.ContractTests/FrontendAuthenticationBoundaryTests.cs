namespace ECommerce.ContractTests;

public sealed class FrontendAuthenticationBoundaryTests
{
    [Fact]
    public void Auth0TokensRemainInMemory()
    {
        string source = File.ReadAllText(GetFrontendPath("lib", "auth", "auth0.ts"));

        Assert.Contains("cacheLocation: \"memory\"", source, StringComparison.Ordinal);
        Assert.Contains("useRefreshTokens: true", source, StringComparison.Ordinal);
        Assert.Contains("useRefreshTokensFallback: true", source, StringComparison.Ordinal);
        Assert.DoesNotContain("cacheLocation: \"localstorage\"", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cacheLocation: \"sessionstorage\"", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FrontendDoesNotWriteAuthenticationDataToBrowserStorage()
    {
        string frontendRoot = GetFrontendPath();
        string[] sourceFiles = Directory
            .EnumerateFiles(frontendRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(frontendRoot, "*.tsx", SearchOption.AllDirectories))
            .Where(path =>
                !path.Contains($"{Path.DirectorySeparatorChar}.next{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                !path.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        foreach (string sourceFile in sourceFiles)
        {
            string source = File.ReadAllText(sourceFile);
            Assert.False(
                ContainsBrowserStorageNearAuthenticationData(source),
                $"Authentication data must not use browser storage: {sourceFile}");
        }
    }

    [Fact]
    public void Auth0RedirectsAreRestrictedToLocalApplicationPaths()
    {
        string source = File.ReadAllText(GetFrontendPath("lib", "auth", "auth0.ts"));

        Assert.Contains(
            "normalizeLocalReturnPath(options.returnTo, \"/\")",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "normalizeLocalReturnPath(returnTo, \"/login\")",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "normalizeLocalReturnPath(result.appState?.returnTo, \"/\")",
            source,
            StringComparison.Ordinal);
        Assert.Contains("!candidate.startsWith(\"/\")", source, StringComparison.Ordinal);
        Assert.Contains("candidate.startsWith(\"//\")", source, StringComparison.Ordinal);
        Assert.Contains("candidate.includes(\"\\\\\")", source, StringComparison.Ordinal);
        Assert.Contains(
            "resolved.origin !== window.location.origin",
            source,
            StringComparison.Ordinal);
    }

    private static bool ContainsBrowserStorageNearAuthenticationData(string source)
    {
        string normalized = string.Concat(source.Where(character => !char.IsWhiteSpace(character)));
        string[] storageOperations =
        [
            "localStorage.setItem(",
            "localStorage.getItem(",
            "sessionStorage.setItem(",
            "sessionStorage.getItem("
        ];
        string[] authenticationTerms =
        [
            "accessToken",
            "refreshToken",
            "idToken",
            "\"token\"",
            "'token'",
            "\"auth\"",
            "'auth'"
        ];

        return storageOperations.Any(operation =>
        {
            int operationIndex = normalized.IndexOf(operation, StringComparison.Ordinal);
            while (operationIndex >= 0)
            {
                int segmentLength = Math.Min(256, normalized.Length - operationIndex);
                string segment = normalized.Substring(operationIndex, segmentLength);
                if (authenticationTerms.Any(term =>
                    segment.Contains(term, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                operationIndex = normalized.IndexOf(
                    operation,
                    operationIndex + operation.Length,
                    StringComparison.Ordinal);
            }

            return false;
        });
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

        return Path.Combine(
            [root, "src", "Frontend", .. segments]);
    }
}
