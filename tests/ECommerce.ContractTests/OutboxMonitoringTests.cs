using System.Diagnostics.Metrics;
using ECommerce.BuildingBlocks.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ECommerce.ContractTests;

public sealed class OutboxMonitoringTests
{
    [Fact]
    public void MonitoringOptionsClampPollingAndWarningIntervals()
    {
        EventBusMonitoringOptions belowMinimum = new()
        {
            OutboxPollIntervalSeconds = 1,
            OutboxWarningAgeSeconds = 1
        };
        EventBusMonitoringOptions aboveMaximum = new()
        {
            OutboxPollIntervalSeconds = 600,
            OutboxWarningAgeSeconds = 7200
        };

        Assert.Equal(TimeSpan.FromSeconds(5), belowMinimum.PollInterval);
        Assert.Equal(TimeSpan.FromSeconds(30), belowMinimum.WarningAge);
        Assert.Equal(TimeSpan.FromSeconds(300), aboveMaximum.PollInterval);
        Assert.Equal(TimeSpan.FromSeconds(3600), aboveMaximum.WarningAge);
    }

    [Fact]
    public void OutboxMetricsPublishCurrentSnapshotAndPollingErrors()
    {
        string meterName = $"ECommerce.ContractTests.{Guid.NewGuid():N}";
        long? pendingMessages = null;
        double? oldestMessageAge = null;
        long pollErrors = 0;
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == meterName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "ecommerce.messaging.outbox.pending")
            {
                pendingMessages = measurement;
            }
            else if (instrument.Name == "ecommerce.messaging.outbox.poll.errors")
            {
                pollErrors += measurement;
            }
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "ecommerce.messaging.outbox.oldest.age")
            {
                oldestMessageAge = measurement;
            }
        });
        listener.Start();
        using OutboxMetrics metrics = new(meterName);

        metrics.Update(4, 75.5);
        metrics.PollErrors.Add(1);
        listener.RecordObservableInstruments();

        Assert.Equal(4, pendingMessages);
        Assert.Equal(75.5, oldestMessageAge);
        Assert.Equal(1, pollErrors);
    }

    [Fact]
    public async Task ObserveAsyncRecordsPollingErrorWhenDbContextCannotBeResolved()
    {
        string meterName = $"ECommerce.ContractTests.{Guid.NewGuid():N}";
        long pollErrors = 0;
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == meterName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "ecommerce.messaging.outbox.poll.errors")
            {
                pollErrors += measurement;
            }
        });
        listener.Start();
        using OutboxMetrics metrics = new(meterName);
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        OutboxMonitoringService<OutboxMonitoringTestDbContext> service = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            metrics,
            Options.Create(new EventBusMonitoringOptions()),
            NullLogger<OutboxMonitoringService<OutboxMonitoringTestDbContext>>.Instance);

        await service.ObserveAsync(CancellationToken.None);

        Assert.Equal(1, pollErrors);
    }
}
