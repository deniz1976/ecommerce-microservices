using System.Text.Json;
using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;
using StackExchange.Redis;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisActiveBasketStore : IActiveBasketStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly BasketExpirationPolicy expirationPolicy;
    private readonly BasketStoreMetrics metrics;

    public RedisActiveBasketStore(
        IConnectionMultiplexer connectionMultiplexer,
        BasketExpirationPolicy expirationPolicy,
        BasketStoreMetrics metrics)
    {
        this.connectionMultiplexer = connectionMultiplexer;
        this.expirationPolicy = expirationPolicy;
        this.metrics = metrics;
    }

    public async Task<BasketEntity?> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
        RedisValue value = await database
            .StringGetAsync(CreateKey(customerId))
            .WaitAsync(cancellationToken);

        if (value.IsNullOrEmpty)
        {
            metrics.RecordReadMiss();
            return null;
        }

        RedisBasketDocument? document = JsonSerializer.Deserialize<RedisBasketDocument>((string)value!, JsonOptions);

        if (document is null)
        {
            metrics.RecordReadMiss();
            return null;
        }

        metrics.RecordReadHit();

        BasketItem[] items = document.Items
            .Select(item => new BasketItem(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Currency,
                item.StoreId))
            .ToArray();

        return new BasketEntity(
            document.CustomerId,
            document.Currency,
            document.CreatedAt,
            document.UpdatedAt,
            items);
    }

    public async Task SaveAsync(BasketEntity basket, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
        TimeSpan? remainingLifetime = expirationPolicy.GetRemainingLifetime(basket);

        if (remainingLifetime is null)
        {
            await database
                .KeyDeleteAsync(CreateKey(basket.CustomerId))
                .WaitAsync(cancellationToken);
            metrics.RecordExpiredBeforeSave();
            return;
        }

        RedisBasketDocument document = new(
            basket.CustomerId,
            basket.Currency,
            basket.CreatedAt,
            basket.UpdatedAt,
            basket.Items.Select(x => new RedisBasketItemDocument(
                x.ProductId,
                x.ProductName,
                x.Quantity,
                x.UnitPrice,
                x.Currency,
                x.StoreId)).ToArray());

        string value = JsonSerializer.Serialize(document, JsonOptions);

        bool saved = await database
            .StringSetAsync(CreateKey(basket.CustomerId), value, remainingLifetime.Value)
            .WaitAsync(cancellationToken);

        if (!saved)
        {
            throw new InvalidOperationException("Redis did not persist the active basket.");
        }

        metrics.RecordWrite();
    }

    public async Task DeleteAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
        bool deleted = await database
            .KeyDeleteAsync(CreateKey(customerId))
            .WaitAsync(cancellationToken);

        if (deleted)
        {
            metrics.RecordDelete();
        }
    }

    private static string CreateKey(Guid customerId)
    {
        return $"basket:{customerId:N}";
    }
}
