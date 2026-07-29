using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Infrastructure.Persistence;

internal static class RoleReconciliationRetentionPolicy
{
    public static DateTimeOffset GetCutoff(
        DateTimeOffset now,
        int retentionDays)
    {
        return now.ToUniversalTime().AddDays(-retentionDays);
    }

    public static IQueryable<Guid> SelectExpiredJobIds(
        IQueryable<RoleReconciliationJob> jobs,
        DateTimeOffset cutoff,
        int batchSize)
    {
        return jobs
            .Where(job => job.CompletedAt != null && job.CompletedAt <= cutoff)
            .OrderBy(job => job.CompletedAt)
            .ThenBy(job => job.Id)
            .Select(job => job.Id)
            .Take(batchSize);
    }
}
