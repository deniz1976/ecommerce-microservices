using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    void Add(Order order);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
