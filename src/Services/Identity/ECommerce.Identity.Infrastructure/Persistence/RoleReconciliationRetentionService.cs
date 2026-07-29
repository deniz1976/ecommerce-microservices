using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationRetentionService
{
    private readonly IdentityDbContext dbContext;
    private readonly RoleReconciliationRetentionOptions options;
    private readonly TimeProvider timeProvider;

    public RoleReconciliationRetentionService(
        IdentityDbContext dbContext,
        IOptions<RoleReconciliationRetentionOptions> options,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public async Task<int> DeleteExpiredCompletedAsync(
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return 0;
        }

        DateTimeOffset cutoff = RoleReconciliationRetentionPolicy.GetCutoff(
            timeProvider.GetUtcNow(),
            options.RetentionDays);
        Guid[] expiredJobIds = await RoleReconciliationRetentionPolicy
            .SelectExpiredJobIds(
                dbContext.RoleReconciliationJobs.AsNoTracking(),
                cutoff,
                options.BatchSize)
            .ToArrayAsync(cancellationToken);

        if (expiredJobIds.Length == 0)
        {
            return 0;
        }

        return await dbContext.RoleReconciliationJobs
            .Where(job =>
                expiredJobIds.Contains(job.Id) &&
                job.CompletedAt != null &&
                job.CompletedAt <= cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
