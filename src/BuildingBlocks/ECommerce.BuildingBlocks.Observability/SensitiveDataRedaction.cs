namespace ECommerce.BuildingBlocks.Observability;

internal static class SensitiveDataRedaction
{
    internal const string RedactedValue = "[REDACTED]";

    private static readonly string[] SensitiveKeyFragments =
    [
        "authorization",
        "cookie",
        "password",
        "passwd",
        "secret",
        "token",
        "api_key",
        "api-key",
        "apikey",
        "connectionstring",
        "connection_string",
        "creditcard",
        "credit_card",
        "card.number",
        "cvv",
        "cvc"
    ];

    private static readonly HashSet<string> SensitiveValueKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "db.statement",
        "http.target",
        "http.url",
        "url.full",
        "url.query"
    };

    internal static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || SensitiveValueKeys.Contains(key))
        {
            return SensitiveValueKeys.Contains(key);
        }

        return SensitiveKeyFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
