using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class ProductImageDeletionQueue : IProductImageDeletionQueue
{
    private readonly CatalogDbContext dbContext;
    private readonly TimeProvider timeProvider;

    public ProductImageDeletionQueue(
        CatalogDbContext dbContext,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.timeProvider = timeProvider;
    }

    public async Task EnqueueAsync(
        string publicId,
        CancellationToken cancellationToken)
    {
        bool alreadyTracked = dbContext.ProductImageDeletionJobs.Local
            .Any(job => job.PublicId == publicId);
        bool alreadyPersisted = alreadyTracked ||
            await dbContext.ProductImageDeletionJobs
                .AnyAsync(job => job.PublicId == publicId, cancellationToken);
        if (alreadyPersisted)
        {
            return;
        }

        dbContext.ProductImageDeletionJobs.Add(
            new ProductImageDeletionJob(
                Guid.NewGuid(),
                publicId,
                timeProvider.GetUtcNow()));
    }

    public async Task MarkCompletedAsync(
        string publicId,
        CancellationToken cancellationToken)
    {
        await dbContext.ProductImageDeletionJobs
            .Where(job => job.PublicId == publicId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
