using OpenTelemetry;
using OpenTelemetry.Logs;

namespace ECommerce.BuildingBlocks.Observability;

internal sealed class SensitiveLogRecordProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord logRecord)
    {
        logRecord.Body = SensitiveLogTextRedaction.Redact(logRecord.Body);
        logRecord.FormattedMessage = SensitiveLogTextRedaction.Redact(logRecord.FormattedMessage);

        if (logRecord.Exception is not null &&
            SensitiveLogTextRedaction.ContainsSensitiveValue(logRecord.Exception.ToString()))
        {
            logRecord.Exception = new RedactedLogException(logRecord.Exception);
        }

        if (logRecord.Attributes is not null)
        {
            logRecord.Attributes = new RedactedLogAttributes(logRecord.Attributes);
        }
    }
}
