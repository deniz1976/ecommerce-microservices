using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.References;

public sealed class CatalogReferenceService
{
    private readonly ICatalogReferenceRepository repository;

    public CatalogReferenceService(ICatalogReferenceRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyCollection<CatalogCategoryResponse>> GetCategoriesAsync(
        string culture,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Category> categories = await repository.GetActiveCategoriesAsync(cancellationToken);
        return categories
            .Select(category => new CatalogCategoryResponse(category.Id, ResolveName(category, culture), category.Slug))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CatalogBrandResponse>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Brand> brands = await repository.GetActiveBrandsAsync(cancellationToken);
        return brands.Select(brand => new CatalogBrandResponse(brand.Id, brand.Name, brand.Slug)).ToArray();
    }

    private static string ResolveName(Category category, string culture)
    {
        CategoryTranslation? translation = category.Translations.FirstOrDefault(item =>
            item.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));
        translation ??= category.Translations.FirstOrDefault(item =>
            item.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase));
        translation ??= category.Translations.FirstOrDefault();
        return translation?.Name ?? category.Slug;
    }
}
