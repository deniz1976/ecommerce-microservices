using ECommerce.Basket.Api.Baskets;
using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application.Products;
using ECommerce.Identity.Api.Auth;
using ECommerce.Identity.Api.Users;
using ECommerce.Identity.Application.Users;
using ECommerce.Inventory.Api.Inventory;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Ordering.Api.Orders;
using ECommerce.Ordering.Application.Orders;
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
    public void DocumentedPublicServiceRoutesExplicitlyAllowAnonymous()
    {
        using WebApplication app = BuildApplication();
        app.MapProductEndpoints();
        app.MapInventoryEndpoints();
        app.MapUserEndpoints();
        app.MapAuthEndpoints();

        AssertAnonymous(app, "/api/v1/products/", "GET");
        AssertAnonymous(app, "/api/v1/products/{id:guid}", "GET");
        AssertAnonymous(app, "/api/v1/inventory/items/{productId:guid}", "GET");
        AssertAnonymous(app, "/api/v1/users/", "POST");
    }

    private static WebApplication BuildApplication()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<BasketService>();
        builder.Services.AddScoped<ProductService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<InventoryService>();
        builder.Services.AddScoped<OrderService>();
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

    private static IEnumerable<RouteEndpoint> Routes(WebApplication app)
    {
        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>();
    }
}
