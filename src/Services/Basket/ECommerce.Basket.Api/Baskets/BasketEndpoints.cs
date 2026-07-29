using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Application.Commands.AddBasketItem;
using ECommerce.Basket.Application.Commands.CheckoutBasket;
using ECommerce.Basket.Application.Commands.ClearBasket;
using ECommerce.Basket.Application.Commands.RemoveBasketItem;
using ECommerce.Basket.Application.Queries.GetBasket;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;

namespace ECommerce.Basket.Api.Baskets;

public static class BasketEndpoints
{
    public static IEndpointRouteBuilder MapBasketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/baskets")
            .WithTags("Baskets")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);

        group.MapGet("/{customerId:guid}", GetAsync)
            .WithName("GetBasket");

        group.MapPut("/{customerId:guid}/items", AddItemAsync)
            .WithName("AddBasketItem");

        group.MapDelete("/{customerId:guid}/items/{productId:guid}", RemoveItemAsync)
            .WithName("RemoveBasketItem");

        group.MapDelete("/{customerId:guid}", ClearAsync)
            .WithName("ClearBasket");

        group.MapPost("/{customerId:guid}/checkout", CheckoutAsync)
            .WithName("CheckoutBasket");

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid customerId,
        IQueryHandler<GetBasketQuery, Result<BasketResponse>> queryHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(httpContext);
        }

        Result<BasketResponse> result = await queryHandler.HandleAsync(
            new GetBasketQuery(customerId),
            cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> AddItemAsync(
        Guid customerId,
        AddBasketItemRequest request,
        ICommandHandler<AddBasketItemCommand, Result<BasketResponse>> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(httpContext);
        }

        Result<BasketResponse> result = await commandHandler.HandleAsync(
            new AddBasketItemCommand(customerId, request),
            cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> RemoveItemAsync(
        Guid customerId,
        Guid productId,
        ICommandHandler<RemoveBasketItemCommand, Result<BasketResponse>> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(httpContext);
        }

        Result<BasketResponse> result = await commandHandler.HandleAsync(
            new RemoveBasketItemCommand(customerId, productId),
            cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> ClearAsync(
        Guid customerId,
        ICommandHandler<ClearBasketCommand, Result> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(httpContext);
        }

        await commandHandler.HandleAsync(new ClearBasketCommand(customerId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CheckoutAsync(
        Guid customerId,
        CheckoutBasketRequest request,
        ICommandHandler<CheckoutBasketCommand, Result<CheckoutBasketResponse>> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return BasketResults.Forbidden(httpContext);
        }

        Result<CheckoutBasketResponse> result = await commandHandler.HandleAsync(
            new CheckoutBasketCommand(customerId, request),
            cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }
}
