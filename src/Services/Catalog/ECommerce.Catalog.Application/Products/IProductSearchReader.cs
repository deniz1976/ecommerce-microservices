using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public interface IProductSearchReader
{
    Task<PagedResult<Product>> SearchAsync(
        ProductListQuery query,
        CancellationToken cancellationToken);
}
