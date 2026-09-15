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

    private readonly IProductReferenceReader referenceReader;

    public ProductReferenceValidator(IProductReferenceReader referenceReader)
    {
        this.referenceReader = referenceReader;
    }

    public async Task<Result> ValidateAsync(
        Guid categoryId,
        Guid brandId,
        IReadOnlyCollection<ProductTranslationInput> translations,
        IReadOnlyCollection<string> existingLanguageCodes,
        CancellationToken cancellationToken)
    {
        if (!await referenceReader.CategoryExistsAsync(categoryId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.CategoryNotFound, CatalogErrorCodes.CategoryNotFound));
        }

        if (!await referenceReader.BrandExistsAsync(brandId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.BrandNotFound, CatalogErrorCodes.BrandNotFound));
        }

        bool hasEnglish =
            translations.Any(x => x.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase)) ||
            existingLanguageCodes.Any(x => x.Equals("en", StringComparison.OrdinalIgnoreCase));
        bool allLanguagesSupported = translations.All(x => SupportedLanguages.Contains(x.LanguageCode));

        return hasEnglish && allLanguagesSupported
            ? Result.Success()
            : Result.Failure(new Error(CatalogErrorCodes.InvalidProductTranslation, CatalogErrorCodes.InvalidProductTranslation));
    }
}
