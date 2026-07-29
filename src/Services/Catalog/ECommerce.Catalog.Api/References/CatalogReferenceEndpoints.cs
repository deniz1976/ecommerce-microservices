using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application.Queries.GetCatalogBrands;
using ECommerce.Catalog.Application.Queries.GetCatalogCategories;
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
        IQueryHandler<
            GetCatalogCategoriesQuery,
            IReadOnlyCollection<CatalogCategoryResponse>> queryHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        return queryHandler.HandleAsync(
            new GetCatalogCategoriesQuery(RequestCultureReader.Read(httpContext)),
            cancellationToken);
    }

    private static Task<IReadOnlyCollection<CatalogBrandResponse>> GetBrandsAsync(
        IQueryHandler<
            GetCatalogBrandsQuery,
            IReadOnlyCollection<CatalogBrandResponse>> queryHandler,
        CancellationToken cancellationToken)
    {
        return queryHandler.HandleAsync(new GetCatalogBrandsQuery(), cancellationToken);
    }
}
