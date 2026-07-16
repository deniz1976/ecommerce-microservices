using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public interface IOrderSubmittedPublisher
{
    Task PublishAsync(Order order, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);
}
