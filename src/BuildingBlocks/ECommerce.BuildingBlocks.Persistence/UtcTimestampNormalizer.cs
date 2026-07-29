namespace ECommerce.BuildingBlocks.Persistence;

internal static class UtcTimestampNormalizer
{
    private const long PostgreSqlTicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    public static DateTimeOffset Normalize(DateTimeOffset value)
    {
        long utcTicks = value.UtcTicks;
        long normalizedTicks = utcTicks - (utcTicks % PostgreSqlTicksPerMicrosecond);
        return new DateTimeOffset(normalizedTicks, TimeSpan.Zero);
    }

    public static DateTime Normalize(DateTime value)
    {
        DateTime utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return utcValue.AddTicks(-(utcValue.Ticks % PostgreSqlTicksPerMicrosecond));
    }
}
