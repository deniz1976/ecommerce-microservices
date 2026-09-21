using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public interface IActiveBasketStore
{
    Task<Result<BasketEntity?>> GetAsync(Guid customerId, CancellationToken cancellationToken);

    Task<Result> SaveAsync(BasketEntity basket, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid customerId, CancellationToken cancellationToken);
}
