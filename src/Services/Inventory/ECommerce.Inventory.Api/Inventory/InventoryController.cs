using ECommerce.BuildingBlocks.Security;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Api.Errors;
using ECommerce.Inventory.Application.Commands.UpsertInventoryItem;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Application.Queries.GetInventoryItem;
using ECommerce.Inventory.Application.Queries.SearchManagedInventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Inventory.Api.Inventory;

[ApiController]
[Route("api/v1/inventory")]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet("items/manage", Name = "SearchManagedInventory")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<InventoryItemResponse>> SearchManagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? productId = null,
        [FromQuery] int? maximumAvailableQuantity = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new SearchManagedInventoryQuery(
                new ManagedInventoryListCriteria(
                    pageNumber,
                    pageSize,
                    productId,
                    maximumAvailableQuantity,
                    sortBy,
                    sortDescending)),
            cancellationToken);
    }

    [HttpGet("items/{productId:guid}", Name = "GetInventoryItem")]
    [AllowAnonymous]
    public async Task<IResult> GetItemAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        InventoryItemResponse? item = await sender.Send(
            new GetInventoryItemQuery(productId),
            cancellationToken);
        return item is null ? InventoryResults.ProductNotFound(HttpContext) : Results.Ok(item);
    }

    [HttpPut("items/{productId:guid}", Name = "UpsertInventoryItem")]
    [Authorize(Policy = AuthorizationPolicies.InventoryManage)]
    public async Task<IResult> UpsertAsync(
        Guid productId,
        [FromBody] UpsertInventoryItemBody body,
        CancellationToken cancellationToken)
    {
        if (body.QuantityOnHand < 0)
        {
            return InventoryResults.InvalidQuantity(HttpContext);
        }

        bool bypassProductOwnership =
            User.IsInRole(ApplicationRoles.Admin) ||
            ClaimsPrincipalPermissionEvaluator.HasPermission(
                User,
                ApplicationPermissions.InventoryWrite);
        InventoryWriteAccess access = new(
            bypassProductOwnership,
            bypassProductOwnership ? null : CustomerAccessTokenReader.Read(HttpContext));
        Result<InventoryItemResponse> result = await sender.Send(
            new UpsertInventoryItemCommand(
                new UpsertInventoryItemRequest(productId, body.QuantityOnHand),
                access),
            cancellationToken);
        return InventoryResults.FromResult(result, HttpContext);
    }
}
