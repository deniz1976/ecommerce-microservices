using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed record CreateProductRequest(
    string Sku,
    Guid CategoryId,
    Guid BrandId,
    decimal Price,
    string Currency,
    ProductStatus Status,
    IReadOnlyCollection<ProductTranslationInput> Translations,
    IReadOnlyCollection<ProductImageInput> Images);
