namespace ECommerce.ApiGateway.Logging;

internal sealed class RedactedLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly KeyValuePair<string, object?>[] values;
    private readonly string message;

    public RedactedLogState(IReadOnlyList<KeyValuePair<string, object?>> source, string message)
    {
        values = new KeyValuePair<string, object?>[source.Count];
        for (int index = 0; index < source.Count; index++)
        {
            KeyValuePair<string, object?> entry = source[index];
            values[index] = new KeyValuePair<string, object?>(
                entry.Key,
                SensitiveQueryRedactor.RedactValue(entry.Value));
        }

        this.message = SensitiveQueryRedactor.Redact(message);
    }

    public KeyValuePair<string, object?> this[int index] => values[index];

    public int Count => values.Length;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, object?>>)values).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return message;
    }
}
