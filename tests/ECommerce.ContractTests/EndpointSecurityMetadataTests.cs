using ECommerce.Basket.Api.Baskets;
using ECommerce.Basket.Application;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Metrics;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Api.References;
using ECommerce.Catalog.Api.Stores;
using ECommerce.Identity.Api.Auth;
using ECommerce.Identity.Api.Users;
using ECommerce.Identity.Application;
using ECommerce.Inventory.Api.Inventory;
using ECommerce.Inventory.Application;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Notification.Api.Notifications;
using ECommerce.Notification.Application;
using ECommerce.Ordering.Api.Orders;
using ECommerce.Ordering.Application;
using ECommerce.Payment.Api.Payments;
using ECommerce.Payment.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class EndpointSecurityMetadataTests
{
    [Fact]
    public void BasketRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();
        app.MapBasketEndpoints();

        AssertPolicyOnEveryRoute(app, "/api/v1/baskets", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void OrderingRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();
        app.MapOrderEndpoints();

        AssertPolicyOnEveryRoute(app, "/api/v1/orders", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void PaymentRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();
        app.MapPaymentEndpoints();

        AssertPolicyOnEveryRoute(app, "/api/v1/payments", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void DirectOrderCreationRequiresTrustedOrderWriter()
    {
        using WebApplication app = BuildApplication();
        app.MapOrderEndpoints();

        AssertPolicy(
            app,
            "/api/v1/orders/",
            "POST",
            AuthorizationPolicies.TrustedOrderWrite);
    }

    [Fact]
    public void NotificationHubRequiresAuthenticatedUser()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddSignalR();
        using WebApplication app = builder.Build();
        app.MapHub<NotificationsHub>("/hubs/notifications")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);

        AssertPolicyOnEveryRoute(app, "/hubs/notifications", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void NotificationHistoryRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();
        app.MapNotificationEndpoints();

        AssertPolicyOnEveryRoute(
            app,
            "/api/v1/notifications",
            AuthorizationPolicies.AuthenticatedUser);
    }

    [Theory]
    [InlineData("Basket", "ECommerce.Basket.Api", "Baskets", "BasketEndpoints.cs", 5)]
    [InlineData("Ordering", "ECommerce.Ordering.Api", "Orders", "OrderEndpoints.cs", 3)]
    [InlineData("Payment", "ECommerce.Payment.Api", "Payments", "PaymentEndpoints.cs", 1)]
    [InlineData("Notification", "ECommerce.Notification.Api", "Hubs", "NotificationsHub.cs", 1)]
    [InlineData("Notification", "ECommerce.Notification.Api", "Notifications", "NotificationEndpoints.cs", 2)]
    public void CustomerResourceEndpointsEnforceOwnership(
        string service,
        string project,
        string feature,
        string fileName,
        int expectedCheckCount)
    {
        string root = FindRepositoryRoot();
        string sourcePath = Path.Combine(root, "src", "Services", service, project, feature, fileName);
        string programPath = Path.Combine(root, "src", "Services", service, project, "Program.cs");
        string source = File.ReadAllText(sourcePath);
        string program = File.ReadAllText(programPath);

        Assert.Equal(
            expectedCheckCount,
            source.Split(".CanAccessAsync(", StringSplitOptions.None).Length - 1);
        Assert.Contains("builder.Services.AddCustomerOwnership(builder.Configuration);", program, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentedPublicServiceRoutesExplicitlyAllowAnonymous()
    {
        using WebApplication app = BuildApplication();
        app.MapProductEndpoints();
        app.MapCatalogReferenceEndpoints();
        app.MapStoreEndpoints();
        app.MapInventoryEndpoints();
        app.MapUserEndpoints();
        app.MapAuthEndpoints();

        AssertAnonymous(app, "/api/v1/products/", "GET");
        AssertAnonymous(app, "/api/v1/products/{id:guid}", "GET");
        AssertAnonymous(app, "/api/v1/catalog-references/categories", "GET");
        AssertAnonymous(app, "/api/v1/catalog-references/brands", "GET");
        AssertAnonymous(app, "/api/v1/stores/{id:guid}", "GET");
        AssertAnonymous(app, "/api/v1/inventory/items/{productId:guid}", "GET");
        AssertAnonymous(app, "/api/v1/users/", "POST");
    }

    [Fact]
    public void CatalogWritesRequireSellerOrAdmin()
    {
        using WebApplication app = BuildApplication();
        app.MapProductEndpoints();
        app.MapProductImageEndpoints();
        app.MapStoreEndpoints();

        AssertPolicy(app, "/api/v1/products/", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{id:guid}", "PUT", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images/", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images/{imageId:guid}/main", "PUT", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images/{imageId:guid}", "DELETE", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/stores/", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/stores/mine", "GET", AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void CatalogManagementReadsRequireSellerOrAdmin()
    {
        using WebApplication app = BuildApplication();
        app.MapProductEndpoints();

        AssertPolicy(app, "/api/v1/products/manage", "GET", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/manage/{id:guid}", "GET", AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void CatalogMetricsRequireAdmin()
    {
        using WebApplication app = BuildApplication();
        app.MapCatalogMetricsEndpoints();

        AssertPolicy(app, "/api/v1/catalog-metrics/", "GET", AuthorizationPolicies.Admin);
    }

    [Fact]
    public void UserReadsRequireAdminWhileRegistrationRemainsAnonymous()
    {
        using WebApplication app = BuildApplication();
        app.MapUserEndpoints();

        AssertPolicy(app, "/api/v1/users/", "GET", AuthorizationPolicies.Admin);
        AssertPolicy(app, "/api/v1/users/{id:guid}", "GET", AuthorizationPolicies.Admin);
        AssertAnonymous(app, "/api/v1/users/", "POST");
    }

    [Fact]
    public void EveryServiceApiRouteHasExactlyOneExplicitAccessClassification()
    {
        using WebApplication app = BuildApplication();
        app.MapBasketEndpoints();
        app.MapProductEndpoints();
        app.MapProductImageEndpoints();
        app.MapCatalogReferenceEndpoints();
        app.MapStoreEndpoints();
        app.MapCatalogMetricsEndpoints();
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapInventoryEndpoints();
        app.MapNotificationEndpoints();
        app.MapOrderEndpoints();
        app.MapPaymentEndpoints();

        RouteEndpoint[] routes = Routes(app)
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api/v1/", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.NotEmpty(routes);

        Assert.All(routes, route =>
        {
            bool allowsAnonymous = route.Metadata.GetMetadata<IAllowAnonymous>() is not null;
            bool requiresAuthorization = route.Metadata
                .GetOrderedMetadata<IAuthorizeData>()
                .Any(metadata => !string.IsNullOrWhiteSpace(metadata.Policy));

            Assert.True(
                allowsAnonymous ^ requiresAuthorization,
                $"Route '{route.RoutePattern.RawText}' must be explicitly anonymous or use one named authorization policy, but never both.");
        });
    }

    private static WebApplication BuildApplication()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddBasketApplication();
        builder.Services.AddCatalogApplication();
        builder.Services.AddIdentityApplication();
        builder.Services.AddInventoryApplication();
        builder.Services.AddNotificationApplication();
        builder.Services.AddOrderingApplication();
        builder.Services.AddPaymentApplication();
        builder.Services.AddSingleton<ICustomerOwnershipAuthorizer, AllowAllCustomerOwnershipAuthorizer>();
        builder.Services.AddSingleton<IAuthenticatedUserResolver, FixedAuthenticatedUserResolver>();
        return builder.Build();
    }

    private static void AssertPolicyOnEveryRoute(WebApplication app, string prefix, string policy)
    {
        RouteEndpoint[] routes = Routes(app)
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith(prefix, StringComparison.Ordinal) == true)
            .ToArray();

        Assert.NotEmpty(routes);
        Assert.All(routes, route => Assert.Contains(
            route.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == policy));
    }

    private static void AssertAnonymous(WebApplication app, string pattern, string method)
    {
        RouteEndpoint route = Assert.Single(Routes(app), endpoint =>
            endpoint.RoutePattern.RawText == pattern &&
            endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.Ordinal) == true);

        Assert.NotNull(route.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static void AssertPolicy(WebApplication app, string pattern, string method, string policy)
    {
        RouteEndpoint route = Assert.Single(Routes(app), endpoint =>
            endpoint.RoutePattern.RawText == pattern &&
            endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.Ordinal) == true);

        Assert.Contains(route.Metadata.GetOrderedMetadata<IAuthorizeData>(), metadata => metadata.Policy == policy);
    }

    private static IEnumerable<RouteEndpoint> Routes(WebApplication app)
    {
        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>();
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
