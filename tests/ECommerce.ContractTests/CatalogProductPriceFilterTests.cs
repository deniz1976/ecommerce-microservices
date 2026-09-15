using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class CatalogProductPriceFilterTests
{
    [Fact]
    public async Task PublicProductSearchPreservesPriceBounds()
    {
        FakeProductRepository products = new();
        PublicProductQueryService service = new(products, products);
        ProductListQuery query = new(
            1,
            20,
            null,
            null,
            null,
            null,
            100m,
            500m,
            ProductStatus.Active,
            "price",
            false);

        Result<PagedResult<ProductResponse>> result = await service.SearchAsync(
            query,
            "en",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, products.LastSearchQuery?.MinPrice);
        Assert.Equal(500m, products.LastSearchQuery?.MaxPrice);
    }

    [Fact]
    public async Task ManagedProductSearchPreservesPriceBounds()
    {
        Guid storeId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        FakeProductRepository products = new();
        ManagedProductQueryService service = new(
            products,
            products,
            new ProductStoreAccessValidator(
                new FakeStoreRepository(new Store(storeId, ownerId, "Owned Store", "owned-store"))));
        ProductListQuery query = new(
            1,
            20,
            null,
            null,
            null,
            storeId,
            null,
            250m,
            null,
            "price",
            true);

        Result<PagedResult<ProductResponse>> result = await service.SearchAsync(
            query,
            new ProductAccessContext(ownerId, IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(products.LastSearchQuery?.MinPrice);
        Assert.Equal(250m, products.LastSearchQuery?.MaxPrice);
    }
}
