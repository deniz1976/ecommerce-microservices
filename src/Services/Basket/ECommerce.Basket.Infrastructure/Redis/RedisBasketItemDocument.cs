namespace ECommerce.Basket.Infrastructure.Redis;

public sealed record RedisBasketItemDocument(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    Guid? StoreId = null);
