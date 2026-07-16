using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public interface IActiveBasketStore
{
    Task<BasketEntity?> GetAsync(Guid customerId, CancellationToken cancellationToken);

    Task SaveAsync(BasketEntity basket, CancellationToken cancellationToken);

    Task DeleteAsync(Guid customerId, CancellationToken cancellationToken);
}
