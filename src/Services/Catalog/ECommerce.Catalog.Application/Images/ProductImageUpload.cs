namespace ECommerce.Catalog.Application.Images;

public sealed record ProductImageUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
