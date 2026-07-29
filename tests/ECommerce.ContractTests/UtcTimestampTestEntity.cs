namespace ECommerce.ContractTests;

internal sealed class UtcTimestampTestEntity
{
    public int Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTime FrameworkTimestamp { get; set; }
}
