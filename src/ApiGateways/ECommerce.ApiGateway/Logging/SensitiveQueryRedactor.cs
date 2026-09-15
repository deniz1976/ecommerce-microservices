using System.Text.RegularExpressions;

namespace ECommerce.ApiGateway.Logging;

public static partial class SensitiveQueryRedactor
{
    public const string Placeholder = "[redacted]";

    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        return SensitiveQueryParameter().Replace(value, $"$1={Placeholder}");
    }

    public static object? RedactValue(object? value)
    {
        return value is string text ? Redact(text) : value;
    }

    [GeneratedRegex(
        "(access_token|id_token|refresh_token|code|client_secret)=[^&\\s\"']+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveQueryParameter();
}
