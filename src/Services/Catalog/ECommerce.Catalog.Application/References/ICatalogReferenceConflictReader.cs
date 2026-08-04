namespace ECommerce.Catalog.Application.References;

public interface ICatalogReferenceConflictReader
{
    Task<bool> CategorySlugExistsAsync(
        string slug,
        Guid? excludedCategoryId,
        CancellationToken cancellationToken);

    Task<bool> BrandSlugExistsAsync(
        string slug,
        Guid? excludedBrandId,
        CancellationToken cancellationToken);
}
