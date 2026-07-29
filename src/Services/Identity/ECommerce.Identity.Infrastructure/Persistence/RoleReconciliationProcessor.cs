using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationProcessor
{
    private const int BatchSize = 10;

    private readonly IdentityDbContext dbContext;
    private readonly IExternalRoleSynchronizer roleSynchronizer;
    private readonly TimeProvider timeProvider;
    private readonly RoleReconciliationMetrics metrics;

    public RoleReconciliationProcessor(
        IdentityDbContext dbContext,
        IExternalRoleSynchronizer roleSynchronizer,
        TimeProvider timeProvider,
        RoleReconciliationMetrics metrics)
    {
        this.dbContext = dbContext;
        this.roleSynchronizer = roleSynchronizer;
        this.timeProvider = timeProvider;
        this.metrics = metrics;
    }

    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await UpdateBacklogMetricsAsync(now, cancellationToken);
        RoleReconciliationJob[] jobs = await dbContext.RoleReconciliationJobs
            .Where(job => job.CompletedAt == null && job.NextAttemptAt <= now)
            .OrderBy(job => job.NextAttemptAt)
            .Take(BatchSize)
            .ToArrayAsync(cancellationToken);

        foreach (RoleReconciliationJob job in jobs)
        {
            ExternalRoleSynchronizationResult result =
                await roleSynchronizer.SynchronizeSelfServiceRoleAsync(
                    job.ExternalSubject,
                    job.DesiredRole,
                    job.CurrentExternalRole,
                    cancellationToken);
            bool succeeded = result == ExternalRoleSynchronizationResult.Succeeded;
            if (succeeded)
            {
                job.MarkSucceeded(timeProvider.GetUtcNow());
            }
            else
            {
                job.MarkFailed(timeProvider.GetUtcNow());
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            if (succeeded)
            {
                metrics.RecordSuccess();
            }
            else
            {
                metrics.RecordFailure();
            }
        }

        await UpdateBacklogMetricsAsync(timeProvider.GetUtcNow(), cancellationToken);
        return jobs.Length;
    }

    private async Task UpdateBacklogMetricsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        IQueryable<RoleReconciliationJob> pendingJobs =
            dbContext.RoleReconciliationJobs
                .AsNoTracking()
                .Where(job => job.CompletedAt == null);
        long pendingCount = await pendingJobs.LongCountAsync(cancellationToken);
        DateTimeOffset? oldestCreatedAt = await pendingJobs
            .Select(job => (DateTimeOffset?)job.CreatedAt)
            .MinAsync(cancellationToken);
        double oldestAgeSeconds = oldestCreatedAt.HasValue
            ? Math.Max(0, (now - oldestCreatedAt.Value).TotalSeconds)
            : 0;

        metrics.UpdateBacklog(pendingCount, oldestAgeSeconds);
    }
}
