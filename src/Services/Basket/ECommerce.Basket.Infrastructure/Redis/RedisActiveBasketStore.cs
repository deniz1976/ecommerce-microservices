using System.Text.Json;
using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class RedisActiveBasketStore : IActiveBasketStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly BasketExpirationPolicy expirationPolicy;
    private readonly BasketStoreMetrics metrics;
    private readonly ILogger<RedisActiveBasketStore> logger;

    public RedisActiveBasketStore(
        IConnectionMultiplexer connectionMultiplexer,
        BasketExpirationPolicy expirationPolicy,
        BasketStoreMetrics metrics,
        ILogger<RedisActiveBasketStore> logger)
    {
        this.connectionMultiplexer = connectionMultiplexer;
        this.expirationPolicy = expirationPolicy;
        this.metrics = metrics;
        this.logger = logger;
    }

    public async Task<Result<BasketEntity?>> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        RedisValue value;

        try
        {
            IDatabase database = connectionMultiplexer.GetDatabase();
            value = await database
                .StringGetAsync(CreateKey(customerId))
                .WaitAsync(cancellationToken);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return Unavailable<BasketEntity?>("read", exception);
        }

        if (value.IsNullOrEmpty)
        {
            metrics.RecordReadMiss();
            return Result<BasketEntity?>.Success(null);
        }

        RedisBasketDocument? document = JsonSerializer.Deserialize<RedisBasketDocument>((string)value!, JsonOptions);

        if (document is null)
        {
            metrics.RecordReadMiss();
            return Result<BasketEntity?>.Success(null);
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

        return Result<BasketEntity?>.Success(new BasketEntity(
            document.CustomerId,
            document.Currency,
            document.CreatedAt,
            document.UpdatedAt,
            items));
    }

    public async Task<Result> SaveAsync(BasketEntity basket, CancellationToken cancellationToken)
    {
        TimeSpan? remainingLifetime = expirationPolicy.GetRemainingLifetime(basket);

        try
        {
            IDatabase database = connectionMultiplexer.GetDatabase();

            if (remainingLifetime is null)
            {
                await database
                    .KeyDeleteAsync(CreateKey(basket.CustomerId))
                    .WaitAsync(cancellationToken);
                metrics.RecordExpiredBeforeSave();
                return Result.Success();
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
                return Unavailable("write", null);
            }
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return Unavailable("write", exception);
        }

        metrics.RecordWrite();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid customerId, CancellationToken cancellationToken)
    {
        bool deleted;

        try
        {
            IDatabase database = connectionMultiplexer.GetDatabase();
            deleted = await database
                .KeyDeleteAsync(CreateKey(customerId))
                .WaitAsync(cancellationToken);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return Unavailable("delete", exception);
        }

        if (deleted)
        {
            metrics.RecordDelete();
        }

        return Result.Success();
    }

    private static bool IsTransportFailure(Exception exception)
    {
        return exception is RedisConnectionException or RedisTimeoutException or TimeoutException;
    }

    private Result Unavailable(string operation, Exception? exception)
    {
        LogUnavailable(operation, exception);
        return Result.Failure(StoreUnavailableError);
    }

    private Result<T> Unavailable<T>(string operation, Exception? exception)
    {
        LogUnavailable(operation, exception);
        return Result<T>.Failure(StoreUnavailableError);
    }

    private void LogUnavailable(string operation, Exception? exception)
    {
        metrics.RecordUnavailable();
        logger.LogWarning(
            exception,
            "Active basket store is unavailable during {Operation}.",
            operation);
    }

    private static Error StoreUnavailableError =>
        new(BasketErrorCodes.BasketStoreUnavailable, BasketErrorCodes.BasketStoreUnavailable);

    private static string CreateKey(Guid customerId)
    {
        return $"basket:{customerId:N}";
    }
}
