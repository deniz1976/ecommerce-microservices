using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class ProductImageDeletionProcessor
{
    private const int BatchSize = 10;

    private readonly CatalogDbContext dbContext;
    private readonly IProductImageStorage imageStorage;
    private readonly TimeProvider timeProvider;
    private readonly ProductImageDeletionMetrics metrics;

    public ProductImageDeletionProcessor(
        CatalogDbContext dbContext,
        IProductImageStorage imageStorage,
        TimeProvider timeProvider,
        ProductImageDeletionMetrics metrics)
    {
        this.dbContext = dbContext;
        this.imageStorage = imageStorage;
        this.timeProvider = timeProvider;
        this.metrics = metrics;
    }

    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        await UpdateBacklogMetricsAsync(now, cancellationToken);
        ProductImageDeletionJob[] jobs = await dbContext.ProductImageDeletionJobs
            .Where(job => job.NextAttemptAt <= now)
            .OrderBy(job => job.NextAttemptAt)
            .ThenBy(job => job.Id)
            .Take(BatchSize)
            .ToArrayAsync(cancellationToken);

        foreach (ProductImageDeletionJob job in jobs)
        {
            bool deleted = await imageStorage.DeleteAsync(job.PublicId, cancellationToken);
            if (deleted)
            {
                dbContext.ProductImageDeletionJobs.Remove(job);
            }
            else
            {
                job.MarkFailed(timeProvider.GetUtcNow());
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            if (deleted)
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
        IQueryable<ProductImageDeletionJob> pendingJobs =
            dbContext.ProductImageDeletionJobs.AsNoTracking();
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
