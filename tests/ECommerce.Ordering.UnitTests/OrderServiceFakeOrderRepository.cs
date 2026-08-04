using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class OrderServiceFakeOrderRepository :
    IRepository<Order, Guid>,
    IUnitOfWork
{
    public List<Order> Orders { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Orders.FirstOrDefault(x => x.Id == id));

    public void Add(Order order) => Orders.Add(order);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
