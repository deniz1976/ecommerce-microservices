using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Ordering.Infrastructure.Persistence;

public sealed class OrderReader : IOrderReader
{
    private readonly OrderingDbContext dbContext;

    public OrderReader(OrderingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }
}
