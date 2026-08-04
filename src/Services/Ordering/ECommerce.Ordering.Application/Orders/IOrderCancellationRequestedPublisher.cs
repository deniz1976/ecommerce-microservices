using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public interface IOrderCancellationRequestedPublisher
{
    Task PublishAsync(
        Order order,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken);
}
