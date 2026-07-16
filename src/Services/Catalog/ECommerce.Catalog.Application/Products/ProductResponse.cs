using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    Guid CategoryId,
    string? CategoryName,
    Guid BrandId,
    string? BrandName,
    decimal Price,
    string Currency,
    ProductStatus Status,
    IReadOnlyCollection<ProductImageResponse> Images);
