using ECommerce.Catalog.Application.Images;

namespace ECommerce.ContractTests;

internal sealed class FakeProductImageDeletionQueue : IProductImageDeletionQueue
{
    public List<string> PendingPublicIds { get; } = [];

    public Task EnqueueAsync(string publicId, CancellationToken cancellationToken)
    {
        PendingPublicIds.Add(publicId);
        return Task.CompletedTask;
    }

    public Task MarkCompletedAsync(string publicId, CancellationToken cancellationToken)
    {
        PendingPublicIds.Remove(publicId);
        return Task.CompletedTask;
    }
}
