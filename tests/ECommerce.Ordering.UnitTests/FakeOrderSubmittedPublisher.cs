using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeOrderSubmittedPublisher : IOrderSubmittedPublisher
{
    private readonly OrderServiceFakeOrderRepository repository;

    public FakeOrderSubmittedPublisher(OrderServiceFakeOrderRepository repository)
    {
        this.repository = repository;
    }

    public int PublishCount { get; private set; }

    public Guid? CorrelationId { get; private set; }

    public Guid? CausationId { get; private set; }

    public bool WasPublishedBeforeSave { get; private set; }

    public Task PublishAsync(Order order, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        PublishCount++;
        CorrelationId = correlationId;
        CausationId = causationId;
        WasPublishedBeforeSave = repository.SaveCount == 0;
        return Task.CompletedTask;
    }
}
