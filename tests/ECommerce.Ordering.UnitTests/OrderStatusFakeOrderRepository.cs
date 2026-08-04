using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class OrderStatusFakeOrderRepository :
    IRepository<Order, Guid>,
    IUnitOfWork
{
    private readonly List<Order> orders = [];

    public int SaveCount { get; private set; }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(orders.FirstOrDefault(x => x.Id == id));

    public void Add(Order order) => orders.Add(order);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
