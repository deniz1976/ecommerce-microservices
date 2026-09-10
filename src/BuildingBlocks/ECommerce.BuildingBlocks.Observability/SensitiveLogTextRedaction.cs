using System.Text.RegularExpressions;

namespace ECommerce.BuildingBlocks.Observability;

internal static partial class SensitiveLogTextRedaction
{
    internal static string? Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string redacted = BearerTokenPattern().Replace(value, $"Bearer {SensitiveDataRedaction.RedactedValue}");
        return CredentialPattern().Replace(redacted, match =>
            $"{match.Groups[1].Value}={SensitiveDataRedaction.RedactedValue}");
    }

    internal static bool ContainsSensitiveValue(string value) =>
        !string.Equals(value, Redact(value), StringComparison.Ordinal);

    [GeneratedRegex(@"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]+", RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenPattern();

    [GeneratedRegex(@"(?i)\b(password|passwd|secret|token|api[_-]?key|client_secret|connectionstring)\s*=\s*[^\s;,&]+", RegexOptions.CultureInvariant)]
    private static partial Regex CredentialPattern();
}
