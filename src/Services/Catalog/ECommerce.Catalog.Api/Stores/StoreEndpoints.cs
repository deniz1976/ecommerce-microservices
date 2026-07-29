using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Commands.CreateStore;
using ECommerce.Catalog.Application.Queries.GetStoreById;
using ECommerce.Catalog.Application.Queries.GetStoresByOwner;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Api.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/stores").WithTags("Stores");

        group.MapGet("/mine", GetMineAsync)
            .WithName("GetMyStores")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetStoreById")
            .AllowAnonymous();

        group.MapPost("/", CreateAsync)
            .WithName("CreateStore")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateStoreRequest request,
        ICommandHandler<CreateStoreCommand, Result<StoreResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Guid? ownerUserId = await userResolver.ResolveUserIdAsync(cancellationToken);
        if (ownerUserId is null)
        {
            return CatalogResults.FromResult(
                Result<StoreResponse>.Failure(new Error(CatalogErrorCodes.IdentityResolutionFailed, CatalogErrorCodes.IdentityResolutionFailed)),
                httpContext);
        }

        Result<StoreResponse> result = await commandHandler.HandleAsync(
            new CreateStoreCommand(ownerUserId.Value, request),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/stores/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetMineAsync(
        IQueryHandler<
            GetStoresByOwnerQuery,
            Result<IReadOnlyCollection<StoreResponse>>> queryHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Guid? ownerUserId = await userResolver.ResolveUserIdAsync(cancellationToken);
        if (ownerUserId is null)
        {
            return CatalogResults.FromResult(
                Result<IReadOnlyCollection<StoreResponse>>.Failure(
                    new Error(CatalogErrorCodes.IdentityResolutionFailed, CatalogErrorCodes.IdentityResolutionFailed)),
                httpContext);
        }

        Result<IReadOnlyCollection<StoreResponse>> result = await queryHandler.HandleAsync(
            new GetStoresByOwnerQuery(ownerUserId.Value),
            cancellationToken);
        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryHandler<GetStoreByIdQuery, Result<StoreResponse>> queryHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<StoreResponse> result = await queryHandler.HandleAsync(
            new GetStoreByIdQuery(id),
            cancellationToken);
        return CatalogResults.FromResult(result, httpContext);
    }
}
