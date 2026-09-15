using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Products;

public interface IProductReferenceValidator
{
    Task<Result> ValidateAsync(
        Guid categoryId,
        Guid brandId,
        IReadOnlyCollection<ProductTranslationInput> translations,
        IReadOnlyCollection<string> existingLanguageCodes,
        CancellationToken cancellationToken);
}
