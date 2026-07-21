using OpenTelemetry;
using OpenTelemetry.Logs;

namespace ECommerce.BuildingBlocks.Observability;

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
