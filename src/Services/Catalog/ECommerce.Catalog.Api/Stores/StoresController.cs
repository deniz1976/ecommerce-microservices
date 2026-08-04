using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Commands.CreateStore;
using ECommerce.Catalog.Application.Commands.UpdateStore;
using ECommerce.Catalog.Application.Queries.GetStoreById;
using ECommerce.Catalog.Application.Queries.GetStoresByOwner;
using ECommerce.Catalog.Application.Queries.SearchManagedStores;
using ECommerce.Catalog.Application.Stores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Catalog.Api.Stores;

[ApiController]
[Route("api/v1/stores")]
public sealed class StoresController(
    ISender sender,
    IAuthenticatedUserResolver userResolver) : ControllerBase
{
    [HttpGet("manage", Name = "SearchManagedStores")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<ManagedStoreResponse>> SearchManagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? ownerUserId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new SearchManagedStoresQuery(
                new ManagedStoreListCriteria(
                    pageNumber,
                    pageSize,
                    search,
                    ownerUserId,
                    sortBy,
                    sortDescending)),
            cancellationToken);
    }

    [HttpGet("mine", Name = "GetMyStores")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> GetMineAsync(CancellationToken cancellationToken)
    {
        Guid? ownerUserId = await userResolver.ResolveUserIdAsync(cancellationToken);
        if (ownerUserId is null)
        {
            return CatalogResults.FromResult(
                Result<IReadOnlyCollection<StoreResponse>>.Failure(
                    new Error(
                        CatalogErrorCodes.IdentityResolutionFailed,
                        CatalogErrorCodes.IdentityResolutionFailed)),
                HttpContext);
        }

        Result<IReadOnlyCollection<StoreResponse>> result = await sender.Send(
            new GetStoresByOwnerQuery(ownerUserId.Value),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpGet("{id:guid}", Name = "GetStoreById")]
    [AllowAnonymous]
    public async Task<IResult> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<StoreResponse> result = await sender.Send(
            new GetStoreByIdQuery(id),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpPost(Name = "CreateStore")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> CreateAsync(
        [FromBody] CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        Guid? ownerUserId = await userResolver.ResolveUserIdAsync(cancellationToken);
        if (ownerUserId is null)
        {
            return CatalogResults.FromResult(
                Result<StoreResponse>.Failure(
                    new Error(
                        CatalogErrorCodes.IdentityResolutionFailed,
                        CatalogErrorCodes.IdentityResolutionFailed)),
                HttpContext);
        }

        Result<StoreResponse> result = await sender.Send(
            new CreateStoreCommand(ownerUserId.Value, request),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.Created($"/api/v1/stores/{result.Value!.Id}", result.Value);
    }

    [HttpPut("{id:guid}", Name = "UpdateStore")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateStoreRequest request,
        CancellationToken cancellationToken)
    {
        bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
        Guid? userId = isAdmin
            ? null
            : await userResolver.ResolveUserIdAsync(cancellationToken);
        Result<StoreResponse> result = await sender.Send(
            new UpdateStoreCommand(
                id,
                request,
                new StoreAccessContext(userId, isAdmin)),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }
}
