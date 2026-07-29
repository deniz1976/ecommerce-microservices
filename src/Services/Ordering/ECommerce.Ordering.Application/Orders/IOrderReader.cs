using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public interface IOrderReader
{
    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);
}
