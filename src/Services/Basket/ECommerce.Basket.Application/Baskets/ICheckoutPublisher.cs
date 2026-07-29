using ECommerce.Basket.Domain;

namespace ECommerce.Basket.Application.Baskets;

public interface ICheckoutPublisher
{
    Task PublishAsync(BasketCheckoutSnapshot snapshot, CancellationToken cancellationToken);
}
