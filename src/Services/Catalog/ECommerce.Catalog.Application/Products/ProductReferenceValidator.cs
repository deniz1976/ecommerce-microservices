using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductReferenceValidator : IProductReferenceValidator
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "tr"
    };

    private readonly IProductRepository repository;

    public ProductReferenceValidator(IProductRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result> ValidateAsync(
        Guid categoryId,
        Guid brandId,
        IReadOnlyCollection<ProductTranslationInput> translations,
        CancellationToken cancellationToken)
    {
        if (!await repository.CategoryExistsAsync(categoryId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.CategoryNotFound, CatalogErrorCodes.CategoryNotFound));
        }

        if (!await repository.BrandExistsAsync(brandId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.BrandNotFound, CatalogErrorCodes.BrandNotFound));
        }

        bool hasEnglish = translations.Any(x => x.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase));
        bool allLanguagesSupported = translations.All(x => SupportedLanguages.Contains(x.LanguageCode));

        return hasEnglish && allLanguagesSupported
            ? Result.Success()
            : Result.Failure(new Error(CatalogErrorCodes.InvalidProductTranslation, CatalogErrorCodes.InvalidProductTranslation));
    }
}
