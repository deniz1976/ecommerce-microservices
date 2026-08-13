namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public const int DefaultBasketTtlHours = 72;

    public const int MinimumBasketTtlHours = 1;

    public const int MaximumBasketTtlHours = 720;

    public string? Endpoint { get; init; }

    public int BasketTtlHours { get; init; } = DefaultBasketTtlHours;

    public static bool IsBasketTtlValid(int basketTtlHours)
    {
        return basketTtlHours is >= MinimumBasketTtlHours and <= MaximumBasketTtlHours;
    }
}
