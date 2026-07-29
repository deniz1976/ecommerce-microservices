namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string? Endpoint { get; init; }

    public int BasketTtlHours { get; init; } = 72;
}
