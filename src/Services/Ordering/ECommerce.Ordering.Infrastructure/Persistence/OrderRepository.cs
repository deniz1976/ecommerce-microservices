using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Ordering.Infrastructure.Persistence;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext dbContext;

    public OrderRepository(OrderingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
