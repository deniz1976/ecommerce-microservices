namespace ECommerce.Catalog.Application.Images;

public interface IProductImageDeletionQueue
{
    Task EnqueueAsync(string publicId, CancellationToken cancellationToken);

    Task MarkCompletedAsync(string publicId, CancellationToken cancellationToken);
}
