using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class OrderStatusFakeOrderRepository : IOrderRepository
{
    private readonly List<Order> orders = [];

    public int SaveCount { get; private set; }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(orders.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> result = orders.Where(x => x.CustomerId == customerId).ToArray();
        return Task.FromResult(result);
    }

    public void Add(Order order) => orders.Add(order);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
