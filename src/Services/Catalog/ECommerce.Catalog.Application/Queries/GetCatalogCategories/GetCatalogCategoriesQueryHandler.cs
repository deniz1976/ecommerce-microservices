using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetCatalogCategories;

public sealed class GetCatalogCategoriesQueryHandler
    : IQueryHandler<GetCatalogCategoriesQuery, IReadOnlyCollection<CatalogCategoryResponse>>
{
    private readonly CatalogReferenceService referenceService;

    public GetCatalogCategoriesQueryHandler(CatalogReferenceService referenceService)
    {
        this.referenceService = referenceService;
    }

    public Task<IReadOnlyCollection<CatalogCategoryResponse>> HandleAsync(
        GetCatalogCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        return referenceService.GetCategoriesAsync(query.Culture, cancellationToken);
    }
}
