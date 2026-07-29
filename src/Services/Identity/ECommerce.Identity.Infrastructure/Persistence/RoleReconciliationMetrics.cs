using System.Diagnostics.Metrics;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationMetrics : IDisposable
{
    public const string MeterName = "ECommerce.Identity.Api";

    private readonly Meter meter;
    private readonly Counter<long> successes;
    private readonly Counter<long> failures;
    private long pendingJobs;
    private long oldestJobAgeBits;

    public RoleReconciliationMetrics()
        : this(MeterName)
    {
    }

    public RoleReconciliationMetrics(string meterName)
    {
        meter = new Meter(meterName);
        meter.CreateObservableGauge(
            "ecommerce.identity.role_reconciliation.pending",
            () => Interlocked.Read(ref pendingJobs),
            unit: "{job}",
            description: "Incomplete Auth0 role reconciliation jobs.");
        meter.CreateObservableGauge(
            "ecommerce.identity.role_reconciliation.oldest.age",
            () => BitConverter.Int64BitsToDouble(Interlocked.Read(ref oldestJobAgeBits)),
            unit: "s",
            description: "Age in seconds of the oldest incomplete Auth0 role reconciliation job.");
        successes = meter.CreateCounter<long>(
            "ecommerce.identity.role_reconciliation.successes",
            unit: "{job}",
            description: "Durable Auth0 role reconciliation jobs completed successfully.");
        failures = meter.CreateCounter<long>(
            "ecommerce.identity.role_reconciliation.failures",
            unit: "{job}",
            description: "Durable Auth0 role reconciliation attempts scheduled for retry.");
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
