using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetCatalogBrands;

public sealed class GetCatalogBrandsQueryHandler
    : IQueryHandler<GetCatalogBrandsQuery, IReadOnlyCollection<CatalogBrandResponse>>
{
    private readonly CatalogReferenceService referenceService;

    public GetCatalogBrandsQueryHandler(CatalogReferenceService referenceService)
    {
        this.referenceService = referenceService;
    }

    public Task<IReadOnlyCollection<CatalogBrandResponse>> HandleAsync(
        GetCatalogBrandsQuery query,
        CancellationToken cancellationToken)
    {
        return referenceService.GetBrandsAsync(cancellationToken);
    }
}
