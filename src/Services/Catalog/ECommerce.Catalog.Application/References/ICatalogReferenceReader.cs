using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.References;

public interface ICatalogReferenceReader
{
    Task<IReadOnlyCollection<Category>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Brand>> GetActiveBrandsAsync(
        CancellationToken cancellationToken);
}
