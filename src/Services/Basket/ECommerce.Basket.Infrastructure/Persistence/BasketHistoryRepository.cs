using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Domain;

namespace ECommerce.Basket.Infrastructure.Persistence;

public sealed class BasketHistoryRepository : IBasketHistoryRepository
{
    private readonly BasketDbContext dbContext;

    public BasketHistoryRepository(BasketDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public void Add(BasketCheckoutSnapshot snapshot)
    {
        dbContext.BasketCheckoutSnapshots.Add(snapshot);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
