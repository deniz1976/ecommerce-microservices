using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;

namespace ECommerce.ContractTests;

public sealed class StubCheckoutPublisher : ICheckoutPublisher
{
    public BasketCheckoutSnapshot? PublishedSnapshot { get; private set; }

    public int PublishCount { get; private set; }

    public Task PublishAsync(BasketCheckoutSnapshot snapshot, CancellationToken cancellationToken)
    {
        PublishedSnapshot = snapshot;
        PublishCount++;
        return Task.CompletedTask;
    }
}
