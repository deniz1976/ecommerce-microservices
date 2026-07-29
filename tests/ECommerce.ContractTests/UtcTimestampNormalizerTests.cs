using ECommerce.BuildingBlocks.Persistence;

namespace ECommerce.ContractTests;

public sealed class UtcTimestampNormalizerTests
{
    [Fact]
    public void NormalizeDateTimeOffsetPreservesTheInstantAndUsesZeroOffset()
    {
        DateTimeOffset source = new(2026, 7, 25, 15, 30, 0, TimeSpan.FromHours(3));

        DateTimeOffset result = UtcTimestampNormalizer.Normalize(source);

        Assert.Equal(TimeSpan.Zero, result.Offset);
        Assert.Equal(source.UtcTicks, result.UtcTicks);
        Assert.Equal(12, result.Hour);
    }

    [Fact]
    public void NormalizeDateTimeOffsetTruncatesToPostgresMicrosecondPrecision()
    {
        DateTimeOffset source = new(638890002000000007, TimeSpan.Zero);

        DateTimeOffset result = UtcTimestampNormalizer.Normalize(source);

        Assert.Equal(638890002000000000, result.UtcTicks);
    }

    [Fact]
    public void NormalizeLocalDateTimeConvertsItToUtc()
    {
        DateTime source = new(2026, 7, 25, 15, 30, 0, DateTimeKind.Local);

        DateTime result = UtcTimestampNormalizer.Normalize(source);

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(source.ToUniversalTime(), result);
    }

    [Fact]
    public void NormalizeUnspecifiedDateTimeMarksItAsUtcWithoutChangingClockFields()
    {
        DateTime source = new(2026, 7, 25, 15, 30, 0, DateTimeKind.Unspecified);

        DateTime result = UtcTimestampNormalizer.Normalize(source);

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(source.Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeDateTimeTruncatesToPostgresMicrosecondPrecision()
    {
        DateTime source = new(638890002000000009, DateTimeKind.Utc);

        DateTime result = UtcTimestampNormalizer.Normalize(source);

        Assert.Equal(638890002000000000, result.Ticks);
    }
}
