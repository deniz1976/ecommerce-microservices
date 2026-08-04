using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeOrderCancellationRequestedPublisher(
    OrderServiceFakeOrderRepository repository)
    : IOrderCancellationRequestedPublisher
{
    public int PublishCount { get; private set; }

    public bool WasPublishedBeforeSave { get; private set; }

    public Task PublishAsync(
        Order order,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken)
    {
        PublishCount++;
        WasPublishedBeforeSave = repository.SaveCount == 0;
        return Task.CompletedTask;
    }
}
