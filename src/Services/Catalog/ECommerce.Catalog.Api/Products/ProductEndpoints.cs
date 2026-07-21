using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("/", SearchAsync)
            .WithName("SearchProducts")
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetProductById")
            .AllowAnonymous();

        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateProduct")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        return endpoints;
    }

    private static async Task<IResult> SearchAsync(
        ProductService productService,
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
        ProductListQuery query = new(pageNumber, pageSize, search, categoryId, brandId, storeId, status, sortBy, sortDescending);
        Result<PagedResult<ProductResponse>> result = await productService.SearchAsync(query, culture, cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ProductService productService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        Result<ProductResponse> result = await productService.GetByIdAsync(id, culture, cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        ProductService productService,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductResponse> result = await productService.CreateAsync(request, access, culture, cancellationToken);

        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/products/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ProductService productService,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductResponse> result = await productService.UpdateAsync(id, request, access, culture, cancellationToken);

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
