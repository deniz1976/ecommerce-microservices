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
using ECommerce.Shipping.Api.Shipments;
using ECommerce.Shipping.Application;
using ECommerce.Shipping.Application.Queries.GetShipmentByOrderId;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class CustomerOwnershipEndpointExecutionTests
{
    [Fact]
    public async Task BasketCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<GetBasketQuery, Result<BasketResponse>> handler = new(
            _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddBasketApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/baskets/{customerId:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["customerId"] = Guid.NewGuid().ToString() });

        await AssertErrorAsync(context, 403, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task OrderingCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<
            GetOrdersByCustomerQuery,
            Result<PagedResult<OrderSummaryResponse>>> handler = new(
                _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddOrderingApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/orders/customer/{customerId:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["customerId"] = Guid.NewGuid().ToString() });

        await AssertErrorAsync(context, 403, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task NotificationCustomerMismatch_ReturnsLocalizedForbiddenBeforeQuery()
    {
        StubQueryHandler<
            GetCustomerNotificationsQuery,
            PagedResult<NotificationMessage>> handler = new(
                _ => throw new InvalidOperationException("Query must not run after ownership denial."));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddNotificationApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/notifications/customer/{customerId:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["customerId"] = Guid.NewGuid().ToString() });

        await AssertErrorAsync(context, 403, ErrorCodes.AccessDenied);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task OrderOwnershipMismatch_ReturnsLocalizedNotFound()
    {
        Guid orderId = Guid.NewGuid();
        StubQueryHandler<GetOrderByIdQuery, Result<OrderResponse>> handler = new(
            _ => Result<OrderResponse>.Success(CreateOrder(orderId, Guid.NewGuid())));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddOrderingApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/orders/{id:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["id"] = orderId.ToString() });

        await AssertErrorAsync(context, 404, ErrorCodes.OrderNotFound);
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task PaymentOwnershipMismatch_ReturnsLocalizedNotFound()
    {
        Guid orderId = Guid.NewGuid();
        StubQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>> handler = new(
            _ => Result<PaymentResponse>.Success(CreatePayment(orderId, Guid.NewGuid())));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddPaymentApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/payments/order/{orderId:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["orderId"] = orderId.ToString() });

        await AssertErrorAsync(context, 404, ErrorCodes.PaymentNotFound);
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task ShipmentOwnershipMismatch_ReturnsLocalizedNotFound()
    {
        Guid orderId = Guid.NewGuid();
        StubQueryHandler<GetShipmentByOrderIdQuery, Result<ShipmentResponse>> handler = new(
            _ => Result<ShipmentResponse>.Success(CreateShipment(orderId, Guid.NewGuid())));
        await using WebApplication app = BuildApplication(
            handler,
            services => services.AddShippingApplication());
        app.MapControllers();

        DefaultHttpContext context = await ExecuteAsync(
            app,
            "/api/v1/shipments/order/{orderId:guid}",
            HttpMethods.Get,
            new RouteValueDictionary { ["orderId"] = orderId.ToString() });

        await AssertErrorAsync(context, 404, ErrorCodes.ShipmentNotFound);
        Assert.Equal(1, handler.InvocationCount);
    }

    private static WebApplication BuildApplication<TQuery, TResponse>(
        StubQueryHandler<TQuery, TResponse> handler,
        Action<IServiceCollection>? registerApplication = null)
        where TQuery : IQuery<TResponse>
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BasketsController).Assembly)
            .AddApplicationPart(typeof(NotificationsController).Assembly)
            .AddApplicationPart(typeof(OrdersController).Assembly)
            .AddApplicationPart(typeof(PaymentsController).Assembly)
            .AddApplicationPart(typeof(ShipmentsController).Assembly);
        builder.Services.AddAuthorization();
        builder.Services.AddECommerceLocalization();
        builder.Services.AddSingleton<ICustomerOwnershipAuthorizer, RejectingCustomerOwnershipAuthorizer>();
        registerApplication?.Invoke(builder.Services);
        builder.Services.AddSingleton<IQueryHandler<TQuery, TResponse>>(handler);
        builder.Services.AddSingleton<MediatR.IRequestHandler<TQuery, TResponse>>(handler);
        return builder.Build();
    }

    private static async Task<DefaultHttpContext> ExecuteAsync(
        WebApplication app,
        string routePattern,
        string method,
        RouteValueDictionary routeValues)
    {
        RouteEndpoint endpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate =>
                Normalize(candidate.RoutePattern.RawText) == Normalize(routePattern) &&
                candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.Ordinal) == true);
        DefaultHttpContext context = new()
        {
            RequestServices = app.Services,
            Response = { Body = new MemoryStream() }
        };
        context.Request.Method = method;
        context.Request.RouteValues = routeValues;
        context.Request.Headers.AcceptLanguage = "tr";

        await endpoint.RequestDelegate!(context);
        return context;
    }

    private static string Normalize(string? pattern)
    {
        return $"/{pattern?.Trim('/')}";
    }

    private static async Task AssertErrorAsync(
        DefaultHttpContext context,
        int expectedStatus,
        string expectedCode)
    {
        context.Response.Body.Position = 0;
        using JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedCode, response.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(response.RootElement.GetProperty("message").GetString()));
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

    private static ShipmentResponse CreateShipment(Guid orderId, Guid customerId)
    {
        return new ShipmentResponse(
            Guid.NewGuid(),
            orderId,
            customerId,
            "DEMO-TRACKING",
            ShipmentStatus.Created,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
    }
}
