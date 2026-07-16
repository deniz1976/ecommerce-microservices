using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Api.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/orders")
            .WithTags("Orders");

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrder");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOrderById");

        group.MapGet("/customer/{customerId:guid}", GetByCustomerIdAsync)
            .WithName("GetOrdersByCustomerId");

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateOrderRequest request,
        OrderService orderService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Guid correlationId = CorrelationReader.Read(httpContext);
        Guid? causationId = CausationReader.Read(httpContext);
        Result<OrderResponse> result = await orderService.CreateAsync(request, correlationId, causationId, cancellationToken);

        return result.IsFailure
            ? OrderResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/orders/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, OrderService orderService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        Result<OrderResponse> result = await orderService.GetByIdAsync(id, cancellationToken);
        return OrderResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetByCustomerIdAsync(Guid customerId, OrderService orderService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<OrderResponse>> result = await orderService.GetByCustomerIdAsync(customerId, cancellationToken);
        return OrderResults.FromResult(result, httpContext);
    }
}
