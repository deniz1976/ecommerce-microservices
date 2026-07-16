using System.Collections;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

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

internal sealed class SensitiveActivityProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        foreach (KeyValuePair<string, object?> attribute in activity.TagObjects.ToArray())
        {
            if (SensitiveDataRedaction.IsSensitiveKey(attribute.Key))
            {
                activity.SetTag(attribute.Key, SensitiveDataRedaction.RedactedValue);
            }
        }
    }
}

internal sealed class SensitiveLogRecordProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord logRecord)
    {
        if (logRecord.Attributes is not null)
        {
            logRecord.Attributes = new RedactedLogAttributes(logRecord.Attributes);
        }
    }
}

internal sealed class RedactedLogAttributes : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly IReadOnlyList<KeyValuePair<string, object?>> attributes;

    internal RedactedLogAttributes(IReadOnlyList<KeyValuePair<string, object?>> attributes)
    {
        this.attributes = attributes;
    }

    public int Count => attributes.Count;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            KeyValuePair<string, object?> attribute = attributes[index];
            return SensitiveDataRedaction.IsSensitiveKey(attribute.Key)
                ? new KeyValuePair<string, object?>(attribute.Key, SensitiveDataRedaction.RedactedValue)
                : attribute;
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
