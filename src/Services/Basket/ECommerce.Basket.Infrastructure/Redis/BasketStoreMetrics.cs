using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class BasketStoreMetrics : IDisposable
{
    public const string MeterName = "ECommerce.Basket.Api";

    private readonly Meter meter;
    private readonly Counter<long> readHits;
    private readonly Counter<long> readMisses;
    private readonly Counter<long> writes;
    private readonly Counter<long> deletes;
    private readonly Counter<long> expiredBeforeSave;
    private readonly Counter<long> unavailable;
    private readonly Counter<long> conflicts;

    public BasketStoreMetrics(IOptions<RedisOptions> options)
        : this(MeterName, TimeSpan.FromHours(options.Value.BasketTtlHours))
    {
    }

    public BasketStoreMetrics(string meterName, TimeSpan configuredLifetime)
    {
        meter = new Meter(meterName);
        meter.CreateObservableGauge(
            "ecommerce.basket.active.ttl",
            () => configuredLifetime.TotalSeconds,
            unit: "s",
            description: "Configured active basket lifetime after the last mutation.");
        readHits = meter.CreateCounter<long>(
            "ecommerce.basket.active.read_hits",
            unit: "{basket}",
            description: "Active basket reads resolved from Redis.");
        readMisses = meter.CreateCounter<long>(
            "ecommerce.basket.active.read_misses",
            unit: "{basket}",
            description: "Active basket reads absent from Redis, including expired baskets.");
        writes = meter.CreateCounter<long>(
            "ecommerce.basket.active.writes",
            unit: "{basket}",
            description: "Active basket writes committed to Redis with expiration.");
        deletes = meter.CreateCounter<long>(
            "ecommerce.basket.active.deletes",
            unit: "{basket}",
            description: "Active basket keys removed explicitly from Redis.");
        expiredBeforeSave = meter.CreateCounter<long>(
            "ecommerce.basket.active.expired_before_save",
            unit: "{basket}",
            description: "Active baskets discarded because their mutation-based lifetime elapsed before persistence.");
        unavailable = meter.CreateCounter<long>(
            "ecommerce.basket.active.unavailable",
            unit: "{operation}",
            description: "Active basket operations rejected because the store was unreachable.");
        conflicts = meter.CreateCounter<long>(
            "ecommerce.basket.active.conflicts",
            unit: "{basket}",
            description: "Active basket writes rejected because another request changed the basket first.");
    }

    public void RecordReadHit() => readHits.Add(1);

    public void RecordReadMiss() => readMisses.Add(1);

    public void RecordWrite() => writes.Add(1);

    public void RecordDelete() => deletes.Add(1);

    public void RecordExpiredBeforeSave() => expiredBeforeSave.Add(1);

    public void RecordUnavailable() => unavailable.Add(1);

    public void RecordConflict() => conflicts.Add(1);

    public void Dispose()
    {
        meter.Dispose();
    }
}
