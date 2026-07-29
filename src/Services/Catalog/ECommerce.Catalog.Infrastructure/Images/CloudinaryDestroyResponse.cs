using System.Text.Json.Serialization;

namespace ECommerce.Catalog.Infrastructure.Images;

internal sealed record CloudinaryDestroyResponse(
    [property: JsonPropertyName("result")] string Result);
