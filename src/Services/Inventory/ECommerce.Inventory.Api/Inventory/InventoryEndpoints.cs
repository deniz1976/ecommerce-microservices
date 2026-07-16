using ECommerce.BuildingBlocks.Security;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Api.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/inventory")
            .WithTags("Inventory");

        group.MapGet("/items/{productId:guid}", GetItemAsync)
            .WithName("GetInventoryItem");

        group.MapPut("/items/{productId:guid}", UpsertAsync)
            .WithName("UpsertInventoryItem")
            .RequireAuthorization(AuthorizationPolicies.Admin);

        return endpoints;
    }

    private static async Task<IResult> GetItemAsync(Guid productId, InventoryService inventoryService, CancellationToken cancellationToken)
    {
        InventoryItemResponse? item = await inventoryService.GetItemAsync(productId, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> UpsertAsync(Guid productId, UpsertInventoryItemBody body, InventoryService inventoryService, CancellationToken cancellationToken)
    {
        if (body.QuantityOnHand < 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["quantityOnHand"] = ["Quantity on hand must be zero or greater."]
            });
        }

        InventoryItemResponse item = await inventoryService.UpsertAsync(new UpsertInventoryItemRequest(productId, body.QuantityOnHand), cancellationToken);
        return Results.Ok(item);
    }
}
