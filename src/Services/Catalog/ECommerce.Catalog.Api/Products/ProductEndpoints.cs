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
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateProduct")
            .RequireAuthorization(AuthorizationPolicies.Admin);

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
        ProductStatus? status = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        string culture = RequestCultureReader.Read(httpContext);
        ProductListQuery query = new(pageNumber, pageSize, search, categoryId, brandId, status, sortBy, sortDescending);
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
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        Result<ProductResponse> result = await productService.CreateAsync(request, culture, cancellationToken);

        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/products/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ProductService productService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(httpContext);
        Result<ProductResponse> result = await productService.UpdateAsync(id, request, culture, cancellationToken);

        return CatalogResults.FromResult(result, httpContext);
    }
}
