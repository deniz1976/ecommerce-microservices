using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.ContractTests;

public sealed class InMemoryProductCatalogReader : IProductCatalogReader
{
    private readonly Dictionary<Guid, CatalogProductSnapshot> products = [];

    public static InMemoryProductCatalogReader Matching(ECommerce.Basket.Domain.Basket basket)
    {
        InMemoryProductCatalogReader reader = new();
        foreach (ECommerce.Basket.Domain.BasketItem item in basket.Items)
        {
            reader.Add(new CatalogProductSnapshot(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Currency,
                1,
                item.StoreId));
        }

        return reader;
    }

    public void Add(CatalogProductSnapshot product) => products[product.Id] = product;

    public Task<Result<CatalogProductSnapshot>> GetActiveProductAsync(
        Guid productId,
        CancellationToken cancellationToken) =>
        Task.FromResult(products.TryGetValue(productId, out CatalogProductSnapshot? product)
            ? Result<CatalogProductSnapshot>.Success(product)
            : Result<CatalogProductSnapshot>.Failure(
                new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound)));
}
