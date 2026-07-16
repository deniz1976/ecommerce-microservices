using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed record UpdateProductRequest(
    Guid CategoryId,
    Guid BrandId,
    decimal Price,
    string Currency,
    ProductStatus Status,
    IReadOnlyCollection<ProductTranslationInput> Translations);
