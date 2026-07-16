using ECommerce.Basket.Domain;

namespace ECommerce.Basket.Application.Baskets;

public interface IBasketHistoryRepository
{
    void Add(BasketCheckoutSnapshot snapshot);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
