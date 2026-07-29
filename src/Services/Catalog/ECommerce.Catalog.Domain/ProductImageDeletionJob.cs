namespace ECommerce.Catalog.Domain;

public sealed class ProductImageDeletionJob
{
    private ProductImageDeletionJob()
    {
        PublicId = string.Empty;
    }

    public ProductImageDeletionJob(
        Guid id,
        string publicId,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicId);

        Id = id;
        PublicId = publicId;
        CreatedAt = createdAt.ToUniversalTime();
        NextAttemptAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string PublicId { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public void MarkFailed(DateTimeOffset attemptedAt)
    {
        AttemptCount++;
        int delaySeconds = Math.Min(3600, 30 * (1 << Math.Min(AttemptCount - 1, 7)));
        NextAttemptAt = attemptedAt.ToUniversalTime().AddSeconds(delaySeconds);
    }
}
