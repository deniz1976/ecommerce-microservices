using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.References;

public interface ICatalogReferenceManagementReader
{
    Task<PagedResult<ManagedCatalogCategoryResponse>> GetCategoriesAsync(
        ManagedCatalogCategoryListCriteria criteria,
        CancellationToken cancellationToken);

    Task<PagedResult<ManagedCatalogBrandResponse>> GetBrandsAsync(
        ManagedCatalogBrandListCriteria criteria,
        CancellationToken cancellationToken);
}
