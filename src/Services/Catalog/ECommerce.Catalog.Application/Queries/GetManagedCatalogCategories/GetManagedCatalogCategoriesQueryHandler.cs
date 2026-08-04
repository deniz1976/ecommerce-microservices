using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetManagedCatalogCategories;

public sealed class GetManagedCatalogCategoriesQueryHandler
    : IQueryHandler<
        GetManagedCatalogCategoriesQuery,
        PagedResult<ManagedCatalogCategoryResponse>>
{
    private readonly CatalogReferenceQueryService queryService;

    public GetManagedCatalogCategoriesQueryHandler(
        CatalogReferenceQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<ManagedCatalogCategoryResponse>> HandleAsync(
        GetManagedCatalogCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetCategoriesAsync(query.Criteria, cancellationToken);
    }
}
