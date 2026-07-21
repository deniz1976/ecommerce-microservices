using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public interface IProductImageAttacher
{
    Task<Result> AttachAsync(
        Product product,
        IReadOnlyCollection<ProductImageInput> images,
        CancellationToken cancellationToken);
}
