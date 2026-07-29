using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Ordering.Application.Commands.CreateOrder;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Application.Queries.GetOrderById;
using ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;

namespace ECommerce.Ordering.Api.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrder")
            .RequireAuthorization(AuthorizationPolicies.TrustedOrderWrite);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOrderById");

        group.MapGet("/customer/{customerId:guid}", GetByCustomerIdAsync)
            .WithName("GetOrdersByCustomerId");

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateOrderRequest request,
        ICommandHandler<CreateOrderCommand, Result<OrderResponse>> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(request.CustomerId, cancellationToken))
        {
            return OrderResults.Forbidden(httpContext);
        }

        Guid correlationId = CorrelationReader.Read(httpContext);
        Guid? causationId = CausationReader.Read(httpContext);
        Result<OrderResponse> result = await commandHandler.HandleAsync(
            new CreateOrderCommand(request, correlationId, causationId),
            cancellationToken);

        return result.IsFailure
            ? OrderResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/orders/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryHandler<GetOrderByIdQuery, Result<OrderResponse>> queryHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<OrderResponse> result = await queryHandler.HandleAsync(
            new GetOrderByIdQuery(id),
            cancellationToken);
        if (!result.IsFailure &&
            !await ownershipAuthorizer.CanAccessAsync(result.Value!.CustomerId, cancellationToken))
        {
            result = Result<OrderResponse>.Failure(
                new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound));
        }

        return OrderResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetByCustomerIdAsync(
        Guid customerId,
        IQueryHandler<
            GetOrdersByCustomerQuery,
            Result<IReadOnlyCollection<OrderResponse>>> queryHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return OrderResults.Forbidden(httpContext);
        }

        Result<IReadOnlyCollection<OrderResponse>> result = await queryHandler.HandleAsync(
            new GetOrdersByCustomerQuery(customerId),
            cancellationToken);
        return OrderResults.FromResult(result, httpContext);
    }
}
