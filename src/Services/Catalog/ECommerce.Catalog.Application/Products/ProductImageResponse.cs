namespace ECommerce.Catalog.Application.Products;

public sealed record ProductImageResponse(
    Guid Id,
    string PublicId,
    string Url,
    string SecureUrl,
    int Width,
    int Height,
    string Format,
    int SortOrder,
    bool IsMain);
