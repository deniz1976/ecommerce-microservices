using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.ContractTests;

public sealed class StubProductCatalogReader : IProductCatalogReader
{
    private readonly Result<CatalogProductSnapshot> result;

    public StubProductCatalogReader(Result<CatalogProductSnapshot> result)
    {
        this.result = result;
    }

    public Task<Result<CatalogProductSnapshot>> GetActiveProductAsync(
        Guid productId,
        CancellationToken cancellationToken) => Task.FromResult(result);
}
