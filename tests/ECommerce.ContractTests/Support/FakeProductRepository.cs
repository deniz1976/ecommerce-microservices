using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

internal sealed class FakeProductRepository : IProductRepository
{
    private Product? product;

    public FakeProductRepository(Product? product = null)
    {
        this.product = product;
    }

    public Task<PagedResult<Product>> SearchAsync(ProductListQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(new PagedResult<Product>([], 1, 20, 0));

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(product);

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<bool> BrandExistsAsync(Guid brandId, CancellationToken cancellationToken) => Task.FromResult(true);

    public void Add(Product value) => product = value;

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
