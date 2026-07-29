namespace ECommerce.Catalog.Application.Images;

public sealed record StoredProductImage(
    string PublicId,
    string Url,
    string SecureUrl,
    int Width,
    int Height,
    string Format);
