using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetManagedCatalogBrands;

public sealed class GetManagedCatalogBrandsQueryHandler
    : IQueryHandler<
        GetManagedCatalogBrandsQuery,
        PagedResult<ManagedCatalogBrandResponse>>
{
    private readonly CatalogReferenceQueryService queryService;

    public GetManagedCatalogBrandsQueryHandler(
        CatalogReferenceQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<ManagedCatalogBrandResponse>> HandleAsync(
        GetManagedCatalogBrandsQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetBrandsAsync(query.Criteria, cancellationToken);
    }
}
