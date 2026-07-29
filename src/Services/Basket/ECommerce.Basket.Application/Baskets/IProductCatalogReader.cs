using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Baskets;

public interface IProductCatalogReader
{
    Task<Result<CatalogProductSnapshot>> GetActiveProductAsync(Guid productId, CancellationToken cancellationToken);
}
