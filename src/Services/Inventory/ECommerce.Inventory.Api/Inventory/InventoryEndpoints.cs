using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Inventory.Api.Errors;
using ECommerce.Inventory.Application.Commands.UpsertInventoryItem;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Application.Queries.GetInventoryItem;

namespace ECommerce.Inventory.Api.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/inventory")
            .WithTags("Inventory");

        group.MapGet("/items/{productId:guid}", GetItemAsync)
            .WithName("GetInventoryItem")
            .AllowAnonymous();

        group.MapPut("/items/{productId:guid}", UpsertAsync)
            .WithName("UpsertInventoryItem")
            .RequireAuthorization(AuthorizationPolicies.InventoryWrite);

        return endpoints;
    }

    private static async Task<IResult> GetItemAsync(
        Guid productId,
        IQueryHandler<GetInventoryItemQuery, InventoryItemResponse?> queryHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        InventoryItemResponse? item = await queryHandler.HandleAsync(
            new GetInventoryItemQuery(productId),
            cancellationToken);
        return item is null ? InventoryResults.ProductNotFound(httpContext) : Results.Ok(item);
    }

    private static async Task<IResult> UpsertAsync(
        Guid productId,
        UpsertInventoryItemBody body,
        ICommandHandler<UpsertInventoryItemCommand, InventoryItemResponse> commandHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (body.QuantityOnHand < 0)
        {
            return InventoryResults.InvalidQuantity(httpContext);
        }

        InventoryItemResponse item = await commandHandler.HandleAsync(
            new UpsertInventoryItemCommand(
                new UpsertInventoryItemRequest(productId, body.QuantityOnHand)),
            cancellationToken);
        return Results.Ok(item);
    }
}
