namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string? ConnectionString { get; init; }

    public int BasketTtlHours { get; init; } = 72;
}
