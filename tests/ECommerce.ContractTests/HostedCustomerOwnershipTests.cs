using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ECommerce.Basket.Api.Baskets;
using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Application.Queries.GetBasket;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Security;
using ECommerce.ContractTests.Support;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Notification.Api.Notifications;
using ECommerce.Notification.Application;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Application.Queries.GetCustomerNotifications;
using ECommerce.Ordering.Api.Orders;
using ECommerce.Ordering.Application;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Application.Queries.GetOrderById;
using ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;
using ECommerce.Ordering.Domain;
using ECommerce.Payment.Api.Payments;
using ECommerce.Payment.Application;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Application.Queries.GetPaymentByOrderId;
using ECommerce.Payment.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.ContractTests;

public sealed class HostedCustomerOwnershipTests
{
    [Fact]
    public async Task AnonymousBasketRequest_ReturnsLocalizedAuthenticationRequired()
    {
        StubQueryHandler<GetBasketQuery, Result<BasketResponse>> handler = new(
            _ => throw new InvalidOperationException("Query must not run without authentication."));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddBasketApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app, authenticated: false);

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/baskets/{Guid.NewGuid()}");

        await AssertErrorAsync(
            response,
            HttpStatusCode.Unauthorized,
            ErrorCodes.AuthenticationRequired);
        Assert.Equal(0, handler.InvocationCount);
        Assert.Contains(
            response.Headers.WwwAuthenticate,
            value => string.Equals(value.Scheme, "Bearer", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BasketCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<GetBasketQuery, Result<BasketResponse>> handler = new(
            _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddBasketApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app);

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/baskets/{Guid.NewGuid()}");

        await AssertErrorAsync(response, HttpStatusCode.Forbidden, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task OrderingCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<
            GetOrdersByCustomerQuery,
            Result<PagedResult<OrderSummaryResponse>>> handler = new(
                _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddOrderingApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app);

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/orders/customer/{Guid.NewGuid()}");

        await AssertErrorAsync(response, HttpStatusCode.Forbidden, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task NotificationCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<
            GetCustomerNotificationsQuery,
            PagedResult<NotificationMessage>> handler = new(
                _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddNotificationApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app);

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/notifications/customer/{Guid.NewGuid()}");

        await AssertErrorAsync(response, HttpStatusCode.Forbidden, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task OrderOwnershipMismatch_ConcealsResourceWithLocalizedNotFound()
    {
        Guid orderId = Guid.NewGuid();
        StubQueryHandler<GetOrderByIdQuery, Result<OrderResponse>> handler = new(
            _ => Result<OrderResponse>.Success(CreateOrder(orderId, Guid.NewGuid())));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddOrderingApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app);

        using HttpResponseMessage response = await client.GetAsync($"/api/v1/orders/{orderId}");

        await AssertErrorAsync(response, HttpStatusCode.NotFound, ErrorCodes.OrderNotFound);
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task PaymentOwnershipMismatch_ConcealsResourceWithLocalizedNotFound()
    {
        Guid orderId = Guid.NewGuid();
        StubQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>> handler = new(
            _ => Result<PaymentResponse>.Success(CreatePayment(orderId, Guid.NewGuid())));
        await using WebApplication app = await StartAsync(
            handler,
            services => services.AddPaymentApplication(),
            endpoints => endpoints.MapControllers());
        using HttpClient client = CreateClient(app);

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/payments/order/{orderId}");

        await AssertErrorAsync(response, HttpStatusCode.NotFound, ErrorCodes.PaymentNotFound);
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task SignalRAnonymousNegotiate_ReturnsLocalizedAuthenticationRequired()
    {
        await using WebApplication app = await StartHubAsync();
        using HttpClient client = CreateClient(app, authenticated: false);

        using HttpResponseMessage response = await client.PostAsync(
            "/hubs/notifications/negotiate?negotiateVersion=1",
            content: null);

        await AssertErrorAsync(
            response,
            HttpStatusCode.Unauthorized,
            ErrorCodes.AuthenticationRequired);
    }

    [Fact]
    public async Task SignalRCustomerMismatch_ReturnsLocalizedHubError()
    {
        await using WebApplication app = await StartHubAsync();
        Uri hubUri = new(GetBaseAddress(app), "/hubs/notifications");
        await using HubConnection connection = new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.AccessTokenProvider = () =>
                    Task.FromResult<string?>(HeaderAuthenticationHandler.Token);
                options.Headers["Accept-Language"] = "tr";
            })
            .Build();
        await connection.StartAsync();

        HubException exception = await Assert.ThrowsAsync<HubException>(
            () => connection.InvokeAsync("JoinCustomerGroup", Guid.NewGuid().ToString()));

        ErrorMessageLocalizer localizer = new();
        Assert.Contains(ErrorCodes.AccessDenied, exception.Message, StringComparison.Ordinal);
        Assert.Contains(
            localizer.GetMessage(ErrorCodes.AccessDenied, "tr"),
            exception.Message,
            StringComparison.Ordinal);
    }

    private static async Task<WebApplication> StartAsync<TQuery, TResponse>(
        StubQueryHandler<TQuery, TResponse> handler,
        Action<IServiceCollection>? registerApplication,
        Action<WebApplication> mapEndpoints)
        where TQuery : IQuery<TResponse>
    {
        WebApplication app = BuildApplication(services =>
        {
            registerApplication?.Invoke(services);
            services.AddSingleton<IQueryHandler<TQuery, TResponse>>(handler);
            services.AddSingleton<MediatR.IRequestHandler<TQuery, TResponse>>(handler);
        });
        app.UseAuthentication();
        app.UseAuthorization();
        mapEndpoints(app);
        await app.StartAsync();
        return app;
    }

    private static WebApplication BuildApplication(
        Action<IServiceCollection>? configureServices = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
            options.Listen(IPAddress.Loopback, 0));
        builder.Services.AddRouting();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BasketsController).Assembly)
            .AddApplicationPart(typeof(NotificationsController).Assembly)
            .AddApplicationPart(typeof(OrdersController).Assembly)
            .AddApplicationPart(typeof(PaymentsController).Assembly);
        builder.Services.AddECommerceLocalization();
        builder.Services
            .AddAuthentication(HeaderAuthenticationHandler.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(
                HeaderAuthenticationHandler.AuthenticationScheme,
                _ => { });
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                AuthorizationPolicies.TrustedOrderWrite,
                policy => policy.RequireAuthenticatedUser());
        builder.Services.Replace(ServiceDescriptor.Singleton<
            IAuthorizationMiddlewareResultHandler,
            LocalizedAuthorizationMiddlewareResultHandler>());
        builder.Services.AddSingleton<
            ICustomerOwnershipAuthorizer,
            RejectingCustomerOwnershipAuthorizer>();
        configureServices?.Invoke(builder.Services);
        return builder.Build();
    }

    private static async Task<WebApplication> StartHubAsync()
    {
        WebApplication app = BuildApplication(services => services.AddSignalR());
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub<NotificationsHub>("/hubs/notifications")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);
        await app.StartAsync();
        return app;
    }

    private static HttpClient CreateClient(WebApplication app, bool authenticated = true)
    {
        HttpClient client = new()
        {
            BaseAddress = GetBaseAddress(app)
        };
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr");
        if (authenticated)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                HeaderAuthenticationHandler.Token);
        }

        return client;
    }

    private static Uri GetBaseAddress(WebApplication app)
    {
        IServer server = app.Services.GetRequiredService<IServer>();
        string address = Assert.Single(
            server.Features.Get<IServerAddressesFeature>()!.Addresses);
        return new Uri(address);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(expectedCode, body.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("message").GetString()));
    }

    private static OrderResponse CreateOrder(Guid orderId, Guid customerId)
    {
        return new OrderResponse(
            orderId,
            customerId,
            "TRY",
            OrderStatus.Submitted,
            10m,
            "Customer",
            "Address",
            "Istanbul",
            "TR",
            "34000",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch,
            [],
            []);
    }

    private static PaymentResponse CreatePayment(Guid orderId, Guid customerId)
    {
        return new PaymentResponse(
            Guid.NewGuid(),
            orderId,
            customerId,
            10m,
            "TRY",
            PaymentStatus.Authorized,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch,
            []);
    }
}
