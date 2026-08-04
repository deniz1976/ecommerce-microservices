using System.Text.Json;

namespace ECommerce.ContractTests;

public sealed class GatewaySecurityConfigurationTests
{
    private static readonly HashSet<string> ExpectedAnonymousRoutes = new(StringComparer.Ordinal)
    {
        "GET /gateway/catalog/products",
        "GET /gateway/catalog/products/{everything}",
        "GET /gateway/catalog/references/categories",
        "GET /gateway/catalog/references/brands",
        "GET /gateway/catalog/stores/{id}",
        "GET /gateway/inventory/{everything}",
        "POST /gateway/users",
        "GET /gateway/health/catalog",
        "GET /gateway/health/basket",
        "GET /gateway/health/ordering",
        "GET /gateway/health/inventory",
        "GET /gateway/health/payment",
        "GET /gateway/health/shipping",
        "GET /gateway/health/notification",
        "GET /gateway/health/identity"
    };

    [Theory]
    [InlineData("ocelot.json")]
    [InlineData("ocelot.Docker.json")]
    public void GatewayIsDefaultDenyWithAnExplicitAnonymousAllowList(string fileName)
    {
        string path = Path.Combine(FindRepositoryRoot(), "src", "ApiGateways", "ECommerce.ApiGateway", fileName);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement root = document.RootElement;

        string[] providerKeys = root
            .GetProperty("GlobalConfiguration")
            .GetProperty("AuthenticationOptions")
            .GetProperty("AuthenticationProviderKeys")
            .EnumerateArray()
            .Select(element => element.GetString() ?? string.Empty)
            .ToArray();
        Assert.Equal(["Bearer"], providerKeys);

        HashSet<string> actualAnonymousRoutes = [];
        foreach (JsonElement route in root.GetProperty("Routes").EnumerateArray())
        {
            bool allowAnonymous = route.TryGetProperty("AuthenticationOptions", out JsonElement authentication) &&
                authentication.TryGetProperty("AllowAnonymous", out JsonElement allowAnonymousElement) &&
                allowAnonymousElement.GetBoolean();
            if (!allowAnonymous)
            {
                continue;
            }

            string pathTemplate = route.GetProperty("UpstreamPathTemplate").GetString() ?? string.Empty;
            foreach (JsonElement method in route.GetProperty("UpstreamHttpMethod").EnumerateArray())
            {
                actualAnonymousRoutes.Add($"{method.GetString()} {pathTemplate}");
            }
        }

        Assert.True(
            ExpectedAnonymousRoutes.SetEquals(actualAnonymousRoutes),
            $"Unexpected anonymous gateway routes: {string.Join(", ", actualAnonymousRoutes.Order())}");
    }

    [Fact]
    public void GatewayDefersRouteAuthorizationToOcelot()
    {
        string path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "ApiGateways",
            "ECommerce.ApiGateway",
            "Program.cs");
        string program = File.ReadAllText(path);

        Assert.Contains("builder.Services.AddOidcReadyAuthentication(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.AddOidcReadySecurity(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.Contains("app.UseECommerceAuthentication();", program, StringComparison.Ordinal);
        Assert.DoesNotContain("app.UseECommerceSecurity();", program, StringComparison.Ordinal);
        Assert.Contains("PreErrorResponderMiddleware", program, StringComparison.Ordinal);
        Assert.Contains(
            "GatewayAuthorizationErrorResponseWriter.WriteIfNeededAsync(context)",
            program,
            StringComparison.Ordinal);
    }

    [Fact]
    public void HostAndDockerGatewayRoutesKeepTheSameSecuritySurface()
    {
        string gatewayRoot = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "ApiGateways",
            "ECommerce.ApiGateway");

        string[] hostRoutes = ReadRouteSecuritySurface(Path.Combine(gatewayRoot, "ocelot.json"));
        string[] dockerRoutes = ReadRouteSecuritySurface(Path.Combine(gatewayRoot, "ocelot.Docker.json"));

        Assert.Equal(hostRoutes, dockerRoutes);
    }

    private static string[] ReadRouteSecuritySurface(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement
            .GetProperty("Routes")
            .EnumerateArray()
            .Select(route =>
            {
                string upstreamPath = route.GetProperty("UpstreamPathTemplate").GetString() ?? string.Empty;
                string downstreamPath = route.GetProperty("DownstreamPathTemplate").GetString() ?? string.Empty;
                string downstreamScheme = route.GetProperty("DownstreamScheme").GetString() ?? string.Empty;
                string methods = string.Join(
                    ",",
                    route.GetProperty("UpstreamHttpMethod")
                        .EnumerateArray()
                        .Select(method => method.GetString() ?? string.Empty)
                        .Order(StringComparer.Ordinal));
                bool allowAnonymous = route.TryGetProperty("AuthenticationOptions", out JsonElement authentication) &&
                    authentication.TryGetProperty("AllowAnonymous", out JsonElement allowAnonymousElement) &&
                    allowAnonymousElement.GetBoolean();
                int priority = route.TryGetProperty("Priority", out JsonElement priorityElement)
                    ? priorityElement.GetInt32()
                    : 0;

                return $"{methods}|{upstreamPath}|{downstreamScheme}|{downstreamPath}|anonymous={allowAnonymous}|priority={priority}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
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
