using System.Diagnostics.Metrics;
using ECommerce.Basket.Infrastructure.Redis;

namespace ECommerce.ContractTests;

public sealed class BasketStoreMetricsTests
{
    [Fact]
    public void MetricsPublishOnlyAggregateUntaggedMeasurements()
    {
        string meterName = $"ECommerce.ContractTests.{Guid.NewGuid():N}";
        Dictionary<string, long> counters = [];
        double? configuredTtl = null;
        bool hasTags = false;
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == meterName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, _) =>
        {
            hasTags |= tags.Length > 0;
            counters[instrument.Name] = counters.GetValueOrDefault(instrument.Name) + measurement;
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _) =>
        {
            hasTags |= tags.Length > 0;
            if (instrument.Name == "ecommerce.basket.active.ttl")
            {
                configuredTtl = measurement;
            }
        });
        listener.Start();
        using BasketStoreMetrics metrics = new(meterName, TimeSpan.FromHours(72));

        metrics.RecordReadHit();
        metrics.RecordReadMiss();
        metrics.RecordWrite();
        metrics.RecordDelete();
        metrics.RecordExpiredBeforeSave();
        listener.RecordObservableInstruments();

        Assert.Equal(72 * 60 * 60, configuredTtl);
        Assert.Equal(1, counters["ecommerce.basket.active.read_hits"]);
        Assert.Equal(1, counters["ecommerce.basket.active.read_misses"]);
        Assert.Equal(1, counters["ecommerce.basket.active.writes"]);
        Assert.Equal(1, counters["ecommerce.basket.active.deletes"]);
        Assert.Equal(1, counters["ecommerce.basket.active.expired_before_save"]);
        Assert.False(hasTags);
    }
}
