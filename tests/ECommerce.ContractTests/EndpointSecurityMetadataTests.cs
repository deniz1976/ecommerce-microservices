using ECommerce.Basket.Api.Baskets;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Identity.Api.Auth;
using ECommerce.Inventory.Api.Inventory;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Notification.Api.Notifications;
using ECommerce.Ordering.Api.Orders;
using ECommerce.Payment.Api.Payments;
using ECommerce.Shipping.Api.Shipments;
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

        AssertPolicyOnEveryRoute(app, "/api/v1/baskets", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void OrderingRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();

        AssertPolicyOnEveryRoute(app, "/api/v1/orders", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void PaymentRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();

        AssertPolicyOnEveryRoute(app, "/api/v1/payments", AuthorizationPolicies.AuthenticatedUser);
    }

    [Fact]
    public void ShippingRoutesRequireAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();

        AssertPolicyOnEveryRoute(
            app,
            "/api/v1/shipments",
            AuthorizationPolicies.AuthenticatedUser);
        AssertPolicy(
            app,
            "/api/v1/shipments/manage",
            "GET",
            AuthorizationPolicies.Admin);
    }

    [Fact]
    public void DirectOrderCreationRequiresTrustedOrderWriter()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/orders",
            "POST",
            AuthorizationPolicies.TrustedOrderWrite);
    }

    [Fact]
    public void OrderCancellationRequiresAuthenticatedUser()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/orders/{id:guid}/cancellation",
            "PUT",
            AuthorizationPolicies.AuthenticatedUser);
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

        AssertPolicyOnEveryRoute(
            app,
            "/api/v1/notifications",
            AuthorizationPolicies.AuthenticatedUser);
    }

    [Theory]
    [InlineData("Basket", "ECommerce.Basket.Api", "Baskets", "BasketsController.cs", 5)]
    [InlineData("Ordering", "ECommerce.Ordering.Api", "Orders", "OrdersController.cs", 4)]
    [InlineData("Payment", "ECommerce.Payment.Api", "Payments", "PaymentsController.cs", 1)]
    [InlineData("Notification", "ECommerce.Notification.Api", "Hubs", "NotificationsHub.cs", 1)]
    [InlineData("Notification", "ECommerce.Notification.Api", "Notifications", "NotificationsController.cs", 3)]
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

        AssertAnonymous(app, "/api/v1/products", "GET");
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

        AssertPolicy(app, "/api/v1/products", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{id:guid}", "PUT", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images/{imageId:guid}/main", "PUT", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/{productId:guid}/images/{imageId:guid}", "DELETE", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/stores", "POST", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/stores/{id:guid}", "PUT", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/stores/mine", "GET", AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void CatalogManagementReadsRequireSellerOrAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/products/manage", "GET", AuthorizationPolicies.SellerOrAdmin);
        AssertPolicy(app, "/api/v1/products/manage/{id:guid}", "GET", AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void CatalogMetricsRequireAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/catalog-metrics", "GET", AuthorizationPolicies.Admin);
    }

    [Fact]
    public void ManagedStoreSearchRequiresAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/stores/manage", "GET", AuthorizationPolicies.Admin);
    }

    [Fact]
    public void ManagedOrderSearchRequiresAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/orders/manage", "GET", AuthorizationPolicies.Admin);
    }

    [Fact]
    public void SellerOrderSearchRequiresSellerOrAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/orders/store/{storeId:guid}",
            "GET",
            AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void SellerOrderDetailRequiresSellerOrAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/orders/store/{storeId:guid}/{orderId:guid}",
            "GET",
            AuthorizationPolicies.SellerOrAdmin);
    }

    [Fact]
    public void ManagedPaymentSearchRequiresAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/payments/manage", "GET", AuthorizationPolicies.Admin);
    }

    [Fact]
    public void InventoryWritesRequireInventoryManager()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/inventory/items/{productId:guid}",
            "PUT",
            AuthorizationPolicies.InventoryManage);
    }

    [Fact]
    public void ManagedInventorySearchRequiresAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/inventory/items/manage",
            "GET",
            AuthorizationPolicies.Admin);
    }

    [Fact]
    public void CatalogReferenceWritesRequireAdmin()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(
            app,
            "/api/v1/catalog-references/categories",
            "POST",
            AuthorizationPolicies.Admin);
        AssertPolicy(
            app,
            "/api/v1/catalog-references/brands",
            "POST",
            AuthorizationPolicies.Admin);
        AssertPolicy(
            app,
            "/api/v1/catalog-references/manage/categories",
            "GET",
            AuthorizationPolicies.Admin);
        AssertPolicy(
            app,
            "/api/v1/catalog-references/manage/brands",
            "GET",
            AuthorizationPolicies.Admin);
        AssertPolicy(
            app,
            "/api/v1/catalog-references/categories/{id:guid}",
            "PUT",
            AuthorizationPolicies.Admin);
        AssertPolicy(
            app,
            "/api/v1/catalog-references/brands/{id:guid}",
            "PUT",
            AuthorizationPolicies.Admin);
    }

    [Fact]
    public void UserReadsRequireAdminWhileRegistrationRemainsAnonymous()
    {
        using WebApplication app = BuildApplication();

        AssertPolicy(app, "/api/v1/users", "GET", AuthorizationPolicies.Admin);
        AssertPolicy(app, "/api/v1/users/{id:guid}", "GET", AuthorizationPolicies.Admin);
        AssertAnonymous(app, "/api/v1/users", "POST");
    }

    [Fact]
    public void EveryServiceApiRouteHasExactlyOneExplicitAccessClassification()
    {
        using WebApplication app = BuildApplication();

        RouteEndpoint[] routes = Routes(app)
            .Where(endpoint => Normalize(endpoint.RoutePattern.RawText)
                .StartsWith("/api/v1/", StringComparison.Ordinal))
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
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BasketsController).Assembly)
            .AddApplicationPart(typeof(ProductsController).Assembly)
            .AddApplicationPart(typeof(AuthController).Assembly)
            .AddApplicationPart(typeof(InventoryController).Assembly)
            .AddApplicationPart(typeof(NotificationsController).Assembly)
            .AddApplicationPart(typeof(OrdersController).Assembly)
            .AddApplicationPart(typeof(PaymentsController).Assembly)
            .AddApplicationPart(typeof(ShipmentsController).Assembly);
        WebApplication app = builder.Build();
        app.MapControllers();
        return app;
    }

    private static void AssertPolicyOnEveryRoute(WebApplication app, string prefix, string policy)
    {
        RouteEndpoint[] routes = Routes(app)
            .Where(endpoint => Normalize(endpoint.RoutePattern.RawText)
                .StartsWith(Normalize(prefix), StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(routes);
        Assert.All(routes, route => Assert.Contains(
            route.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == policy));
    }

    private static void AssertAnonymous(WebApplication app, string pattern, string method)
    {
        RouteEndpoint route = Assert.Single(Routes(app), endpoint =>
            Normalize(endpoint.RoutePattern.RawText) == Normalize(pattern) &&
            endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.Ordinal) == true);

        Assert.NotNull(route.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static void AssertPolicy(WebApplication app, string pattern, string method, string policy)
    {
        RouteEndpoint route = Assert.Single(Routes(app), endpoint =>
            Normalize(endpoint.RoutePattern.RawText) == Normalize(pattern) &&
            endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.Ordinal) == true);

        Assert.Contains(route.Metadata.GetOrderedMetadata<IAuthorizeData>(), metadata => metadata.Policy == policy);
    }

    private static IEnumerable<RouteEndpoint> Routes(WebApplication app)
    {
        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>();
    }

    private static string Normalize(string? pattern)
    {
        return $"/{pattern?.Trim('/')}";
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
