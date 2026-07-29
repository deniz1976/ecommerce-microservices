using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application.Commands.CreateProduct;
using ECommerce.Catalog.Application.Commands.UpdateProduct;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.Queries.GetProduct;
using ECommerce.Catalog.Application.Queries.SearchProducts;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("/", SearchPublicAsync)
            .WithName("SearchProducts")
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetPublicByIdAsync)
            .WithName("GetProductById")
            .AllowAnonymous();

        group.MapGet("/manage", SearchManagedAsync)
            .WithName("SearchManagedProducts")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapGet("/manage/{id:guid}", GetManagedByIdAsync)
            .WithName("GetManagedProductById")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateProduct")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        return endpoints;
    }

    private static async Task<IResult> SearchPublicAsync(
        IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductResponse>>> queryHandler,
        HttpContext httpContext,
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        Guid? storeId = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductListQuery query = new(pageNumber, pageSize, search, categoryId, brandId, storeId, ProductStatus.Active, sortBy, sortDescending);
        Result<PagedResult<ProductResponse>> result = await queryHandler.HandleAsync(
            new SearchProductsQuery(query, culture, Managed: false),
            cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> SearchManagedAsync(
        IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductResponse>>> queryHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        Guid? storeId = null,
        ProductStatus? status = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        ProductListQuery query = new(pageNumber, pageSize, search, categoryId, brandId, storeId, status, sortBy, sortDescending);
        Result<PagedResult<ProductResponse>> result = await queryHandler.HandleAsync(
            new SearchProductsQuery(query, culture, Managed: true, access),
            cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetPublicByIdAsync(
        Guid id,
        IQueryHandler<GetProductQuery, Result<ProductResponse>> queryHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        Result<ProductResponse> result = await queryHandler.HandleAsync(
            new GetProductQuery(id, culture, Managed: false),
            cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetManagedByIdAsync(
        Guid id,
        IQueryHandler<GetProductQuery, Result<ProductResponse>> queryHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductResponse> result = await queryHandler.HandleAsync(
            new GetProductQuery(id, culture, Managed: true, access),
            cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        ICommandHandler<CreateProductCommand, Result<ProductResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductResponse> result = await commandHandler.HandleAsync(
            new CreateProductCommand(request, access, culture),
            cancellationToken);

        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/products/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ICommandHandler<UpdateProductCommand, Result<ProductResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductResponse> result = await commandHandler.HandleAsync(
            new UpdateProductCommand(id, request, access, culture),
            cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<ProductAccessContext> ResolveAccessAsync(
        HttpContext httpContext,
        IAuthenticatedUserResolver userResolver,
        CancellationToken cancellationToken)
    {
        bool isAdmin = httpContext.User.IsInRole(ApplicationRoles.Admin);
        Guid? userId = isAdmin ? null : await userResolver.ResolveUserIdAsync(cancellationToken);
        return new ProductAccessContext(userId, isAdmin);
    }
}
