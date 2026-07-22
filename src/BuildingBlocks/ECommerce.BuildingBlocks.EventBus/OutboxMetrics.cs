using System.Diagnostics.Metrics;

namespace ECommerce.BuildingBlocks.EventBus;

internal sealed class OutboxMetrics : IDisposable
{
    private readonly Meter meter;
    private long pendingMessages;
    private long oldestMessageAgeBits;

    public OutboxMetrics(string meterName)
    {
        meter = new Meter(meterName);
        meter.CreateObservableGauge(
            "ecommerce.messaging.outbox.pending",
            () => Interlocked.Read(ref pendingMessages),
            unit: "{message}",
            description: "Messages waiting in the transactional outbox.");
        meter.CreateObservableGauge(
            "ecommerce.messaging.outbox.oldest.age",
            () => BitConverter.Int64BitsToDouble(Interlocked.Read(ref oldestMessageAgeBits)),
            unit: "s",
            description: "Age in seconds of the oldest message waiting in the transactional outbox.");
        PollErrors = meter.CreateCounter<long>(
            "ecommerce.messaging.outbox.poll.errors",
            unit: "{error}",
            description: "Failures while reading transactional outbox monitoring data.");
    }

    public Counter<long> PollErrors { get; }

    public void Update(long messageCount, double oldestAgeSeconds)
    {
        Interlocked.Exchange(ref pendingMessages, messageCount);
        Interlocked.Exchange(ref oldestMessageAgeBits, BitConverter.DoubleToInt64Bits(oldestAgeSeconds));
    }

    public void Dispose()
    {
        meter.Dispose();
    }
}
