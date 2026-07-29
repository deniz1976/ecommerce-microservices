using System.Diagnostics.Metrics;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class ProductImageDeletionMetrics : IDisposable
{
    public const string MeterName = "ECommerce.Catalog.Api";

    private readonly Meter meter;
    private readonly Counter<long> successes;
    private readonly Counter<long> failures;
    private long pendingJobs;
    private long oldestJobAgeBits;

    public ProductImageDeletionMetrics()
        : this(MeterName)
    {
    }

    public ProductImageDeletionMetrics(string meterName)
    {
        meter = new Meter(meterName);
        meter.CreateObservableGauge(
            "ecommerce.catalog.product_image_deletion.pending",
            () => Interlocked.Read(ref pendingJobs),
            unit: "{job}",
            description: "Pending durable product image provider deletion jobs.");
        meter.CreateObservableGauge(
            "ecommerce.catalog.product_image_deletion.oldest.age",
            () => BitConverter.Int64BitsToDouble(Interlocked.Read(ref oldestJobAgeBits)),
            unit: "s",
            description: "Age in seconds of the oldest pending product image deletion job.");
        successes = meter.CreateCounter<long>(
            "ecommerce.catalog.product_image_deletion.successes",
            unit: "{job}",
            description: "Durable product image provider deletion jobs completed successfully.");
        failures = meter.CreateCounter<long>(
            "ecommerce.catalog.product_image_deletion.failures",
            unit: "{job}",
            description: "Durable product image provider deletion attempts scheduled for retry.");
    }

    public void UpdateBacklog(long pendingJobCount, double oldestJobAgeSeconds)
    {
        Interlocked.Exchange(ref pendingJobs, pendingJobCount);
        Interlocked.Exchange(
            ref oldestJobAgeBits,
            BitConverter.DoubleToInt64Bits(oldestJobAgeSeconds));
    }

    public void RecordSuccess() => successes.Add(1);

    public void RecordFailure() => failures.Add(1);

    public void Dispose()
    {
        meter.Dispose();
    }
}
