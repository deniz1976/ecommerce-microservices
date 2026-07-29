using System.Diagnostics.Metrics;
using ECommerce.Identity.Infrastructure.Persistence;

namespace ECommerce.ContractTests;

public sealed class RoleReconciliationMetricsTests
{
    [Fact]
    public void MetricsPublishOnlyAggregateUntaggedMeasurements()
    {
        string meterName = $"ECommerce.ContractTests.{Guid.NewGuid():N}";
        long? pending = null;
        double? oldestAge = null;
        long successes = 0;
        long failures = 0;
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
            switch (instrument.Name)
            {
                case "ecommerce.identity.role_reconciliation.pending":
                    pending = measurement;
                    break;
                case "ecommerce.identity.role_reconciliation.successes":
                    successes += measurement;
                    break;
                case "ecommerce.identity.role_reconciliation.failures":
                    failures += measurement;
                    break;
            }
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _) =>
        {
            hasTags |= tags.Length > 0;
            if (instrument.Name == "ecommerce.identity.role_reconciliation.oldest.age")
            {
                oldestAge = measurement;
            }
        });
        listener.Start();
        using RoleReconciliationMetrics metrics = new(meterName);

        metrics.UpdateBacklog(3, 45.5);
        metrics.RecordSuccess();
        metrics.RecordFailure();
        listener.RecordObservableInstruments();

        Assert.Equal(3, pending);
        Assert.Equal(45.5, oldestAge);
        Assert.Equal(1, successes);
        Assert.Equal(1, failures);
        Assert.False(hasTags);
    }
}
