using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class OrderServiceFakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Orders.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> orders = Orders.Where(x => x.CustomerId == customerId).ToArray();
        return Task.FromResult(orders);
    }

    public void Add(Order order) => Orders.Add(order);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
