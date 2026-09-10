namespace ECommerce.BuildingBlocks.Observability;

internal sealed class RedactedLogException : Exception
{
    internal RedactedLogException(Exception original)
        : base($"{original.GetType().Name}: sensitive exception details redacted")
    {
    }
}
