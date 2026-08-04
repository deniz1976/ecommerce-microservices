using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.References;

public sealed class CatalogReferenceQueryService
{
    private readonly ICatalogReferenceManagementReader managementReader;

    public CatalogReferenceQueryService(
        ICatalogReferenceManagementReader managementReader)
    {
        this.managementReader = managementReader;
    }

    public Task<PagedResult<ManagedCatalogCategoryResponse>> GetCategoriesAsync(
        ManagedCatalogCategoryListCriteria criteria,
        CancellationToken cancellationToken) =>
        managementReader.GetCategoriesAsync(criteria, cancellationToken);

    public Task<PagedResult<ManagedCatalogBrandResponse>> GetBrandsAsync(
        ManagedCatalogBrandListCriteria criteria,
        CancellationToken cancellationToken) =>
        managementReader.GetBrandsAsync(criteria, cancellationToken);
}
