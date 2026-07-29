using System.Text.Json.Serialization;

namespace ECommerce.Catalog.Infrastructure.Images;

internal sealed record CloudinaryUploadResponse(
    [property: JsonPropertyName("public_id")] string PublicId,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("secure_url")] string SecureUrl,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("format")] string Format);
