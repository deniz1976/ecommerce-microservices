using System.Text.Json;
using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisActiveBasketStore : IActiveBasketStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly RedisOptions options;

    public RedisActiveBasketStore(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> options)
    {
        this.connectionMultiplexer = connectionMultiplexer;
        this.options = options.Value;
    }

    public async Task<BasketEntity?> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
        RedisValue value = await database.StringGetAsync(CreateKey(customerId));

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        RedisBasketDocument? document = JsonSerializer.Deserialize<RedisBasketDocument>((string)value!, JsonOptions);

        if (document is null)
        {
            return null;
        }

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

    public Task SaveAsync(BasketEntity basket, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
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

        return database.StringSetAsync(CreateKey(basket.CustomerId), value, TimeSpan.FromHours(options.BasketTtlHours));
    }

    public Task DeleteAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IDatabase database = connectionMultiplexer.GetDatabase();
        return database.KeyDeleteAsync(CreateKey(customerId));
    }

    private static string CreateKey(Guid customerId)
    {
        return $"basket:{customerId:N}";
    }
}
