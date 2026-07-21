using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

internal sealed class FakeCatalogReferenceRepository : ICatalogReferenceRepository
{
    public IReadOnlyCollection<Category> Categories { get; init; } = [];

    public IReadOnlyCollection<Brand> Brands { get; init; } = [];

    public Task<IReadOnlyCollection<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Categories);
    }

    public Task<IReadOnlyCollection<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Brands);
    }
}
