using System.Diagnostics;
using OpenTelemetry;

namespace ECommerce.BuildingBlocks.Observability;

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
