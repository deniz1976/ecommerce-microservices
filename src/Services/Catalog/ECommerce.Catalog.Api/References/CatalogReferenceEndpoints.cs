using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Api.References;

public static class CatalogReferenceEndpoints
{
    public static IEndpointRouteBuilder MapCatalogReferenceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/catalog-references")
            .WithTags("Catalog References")
            .AllowAnonymous();

        group.MapGet("/categories", GetCategoriesAsync).WithName("GetCatalogCategories");
        group.MapGet("/brands", GetBrandsAsync).WithName("GetCatalogBrands");
        return endpoints;
    }

    private static Task<IReadOnlyCollection<CatalogCategoryResponse>> GetCategoriesAsync(
        CatalogReferenceService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        return service.GetCategoriesAsync(RequestCultureReader.Read(httpContext), cancellationToken);
    }

    private static Task<IReadOnlyCollection<CatalogBrandResponse>> GetBrandsAsync(
        CatalogReferenceService service,
        CancellationToken cancellationToken)
    {
        return service.GetBrandsAsync(cancellationToken);
    }
}
