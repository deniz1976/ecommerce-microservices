using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class ProductImageDeletionJobTests
{
    [Fact]
    public void Constructor_normalizes_timestamps_to_utc()
    {
        DateTimeOffset createdAt =
            new(2026, 7, 28, 15, 0, 0, TimeSpan.FromHours(3));

        ProductImageDeletionJob job = new(
            Guid.NewGuid(),
            "ecommerce/products/item",
            createdAt);

        Assert.Equal(new DateTimeOffset(2026, 7, 28, 12, 0, 0, TimeSpan.Zero), job.CreatedAt);
        Assert.Equal(job.CreatedAt, job.NextAttemptAt);
    }

    [Fact]
    public void Failed_attempts_use_bounded_utc_backoff()
    {
        ProductImageDeletionJob job = new(
            Guid.NewGuid(),
            "ecommerce/products/item",
            DateTimeOffset.UtcNow);
        DateTimeOffset attemptedAt = DateTimeOffset.UtcNow;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            job.MarkFailed(attemptedAt);
        }

        Assert.Equal(10, job.AttemptCount);
        Assert.Equal(attemptedAt.AddHours(1), job.NextAttemptAt);
        Assert.Equal(TimeSpan.Zero, job.NextAttemptAt.Offset);
    }
}
