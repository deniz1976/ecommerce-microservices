using System.Collections;

namespace ECommerce.BuildingBlocks.Observability;

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
