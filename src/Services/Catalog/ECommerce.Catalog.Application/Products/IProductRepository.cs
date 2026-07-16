using ECommerce.Catalog.Domain;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Products;

public interface IProductRepository
{
    Task<PagedResult<Product>> SearchAsync(ProductListQuery query, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<bool> BrandExistsAsync(Guid brandId, CancellationToken cancellationToken);

    void Add(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
