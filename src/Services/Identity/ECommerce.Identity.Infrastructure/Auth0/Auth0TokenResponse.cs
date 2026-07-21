using System.Text.Json.Serialization;

namespace ECommerce.Identity.Infrastructure.Auth0;

internal sealed record Auth0TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
