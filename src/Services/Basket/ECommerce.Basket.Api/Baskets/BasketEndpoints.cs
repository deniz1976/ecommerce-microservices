using ECommerce.Basket.Application.Baskets;
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

    private static async Task<IResult> GetAsync(Guid customerId, BasketService basketService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        Result<BasketResponse> result = await basketService.GetAsync(customerId, cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> AddItemAsync(
        Guid customerId,
        AddBasketItemRequest request,
        BasketService basketService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<BasketResponse> result = await basketService.AddItemAsync(customerId, request, cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> RemoveItemAsync(
        Guid customerId,
        Guid productId,
        BasketService basketService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<BasketResponse> result = await basketService.RemoveItemAsync(customerId, productId, cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> ClearAsync(Guid customerId, BasketService basketService, CancellationToken cancellationToken)
    {
        await basketService.ClearAsync(customerId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CheckoutAsync(Guid customerId, BasketService basketService, HttpContext httpContext, CancellationToken cancellationToken)
    {
        Result<CheckoutBasketResponse> result = await basketService.CheckoutAsync(customerId, cancellationToken);
        return BasketResults.FromResult(result, httpContext);
    }
}
