namespace ECommerce.Basket.Infrastructure.Redis;

public sealed record RedisBasketDocument(
    Guid CustomerId,
    string Currency,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<RedisBasketItemDocument> Items);
