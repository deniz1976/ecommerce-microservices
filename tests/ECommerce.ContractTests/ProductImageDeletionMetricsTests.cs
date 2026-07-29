using System.Diagnostics.Metrics;
using ECommerce.Catalog.Infrastructure.Persistence;

namespace ECommerce.ContractTests;

public sealed class ProductImageDeletionMetricsTests
{
    [Fact]
    public void Metrics_are_aggregate_and_untagged()
    {
        string meterName = $"catalog-image-cleanup-tests-{Guid.NewGuid():N}";
        Dictionary<string, double> measurements = [];
        List<KeyValuePair<string, object?>> observedTags = [];
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, currentListener) =>
        {
            if (instrument.Meter.Name == meterName)
            {
                currentListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, _) =>
        {
            measurements[instrument.Name] = measurement;
            observedTags.AddRange(tags.ToArray());
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _) =>
        {
            measurements[instrument.Name] = measurement;
            observedTags.AddRange(tags.ToArray());
        });
        listener.Start();

        using ProductImageDeletionMetrics metrics = new(meterName);
        metrics.UpdateBacklog(4, 90.25);
        metrics.RecordSuccess();
        metrics.RecordFailure();
        listener.RecordObservableInstruments();

        Assert.Equal(4, measurements["ecommerce.catalog.product_image_deletion.pending"]);
        Assert.Equal(90.25, measurements["ecommerce.catalog.product_image_deletion.oldest.age"]);
        Assert.Equal(1, measurements["ecommerce.catalog.product_image_deletion.successes"]);
        Assert.Equal(1, measurements["ecommerce.catalog.product_image_deletion.failures"]);
        Assert.Empty(observedTags);
    }
}
