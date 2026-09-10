namespace ECommerce.ContractTests;

public sealed class FrontendAuthenticationBoundaryTests
{
    [Fact]
    public void Auth0SessionUsesTheServerSdkAndHardenedCookieSettings()
    {
        string serverSource = File.ReadAllText(
            GetFrontendPath("lib", "auth", "auth0-server.ts"));
        string packageSource = File.ReadAllText(GetFrontendPath("package.json"));

        Assert.Contains(
            "@auth0/nextjs-auth0/server",
            serverSource,
            StringComparison.Ordinal);
        Assert.Contains("sameSite: \"lax\"", serverSource, StringComparison.Ordinal);
        Assert.Contains(
            "secure: process.env.NODE_ENV === \"production\"",
            serverSource,
            StringComparison.Ordinal);
        Assert.Contains("rolling: true", serverSource, StringComparison.Ordinal);
        Assert.Contains("offline_access", serverSource, StringComparison.Ordinal);
        Assert.Contains(
            "\"@auth0/nextjs-auth0\"",
            packageSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "\"@auth0/auth0-spa-js\"",
            packageSource,
            StringComparison.Ordinal);
    }

    [Fact]
    public void BrowserObtainsOnlyAnAccessTokenThroughTheBffEndpoint()
    {
        string clientSource = File.ReadAllText(
            GetFrontendPath("lib", "auth", "auth0.ts"));
        string serverSource = File.ReadAllText(
            GetFrontendPath("lib", "auth", "auth0-server.ts"));

        Assert.Contains(
            "@auth0/nextjs-auth0/client",
            clientSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "return await getBffAccessToken()",
            clientSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "enableAccessTokenEndpoint: true",
            serverSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain("refreshToken", clientSource, StringComparison.Ordinal);
        Assert.DoesNotContain("idToken", clientSource, StringComparison.Ordinal);
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
        Assert.Contains("!candidate.startsWith(\"/\")", source, StringComparison.Ordinal);
        Assert.Contains("candidate.startsWith(\"//\")", source, StringComparison.Ordinal);
        Assert.Contains("candidate.includes(\"\\\\\")", source, StringComparison.Ordinal);
        Assert.Contains(
            "resolved.origin !== window.location.origin",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Auth0ProxyCoversApplicationRoutesForRollingSessions()
    {
        string source = File.ReadAllText(GetFrontendPath("proxy.ts"));

        Assert.Contains(
            "await auth0.middleware(",
            source,
            StringComparison.Ordinal);
        Assert.Contains("new NextRequest(request, { headers: requestHeaders })", source, StringComparison.Ordinal);
        Assert.Contains("response.headers.set(\"Content-Security-Policy\"", source, StringComparison.Ordinal);
        Assert.Contains("_next/static", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ForcedTokenRefreshRequiresSameOriginPost()
    {
        string source = File.ReadAllText(
            GetFrontendPath("app", "auth", "refresh-access-token", "route.ts"));

        Assert.Contains(
            "export async function POST",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "origin === request.nextUrl.origin",
            source,
            StringComparison.Ordinal);
        Assert.Contains("{ refresh: true }", source, StringComparison.Ordinal);
        Assert.Contains("\"Cache-Control\": \"no-store\"", source, StringComparison.Ordinal);
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
