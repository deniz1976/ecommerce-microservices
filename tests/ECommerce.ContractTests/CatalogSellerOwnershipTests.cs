using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class CatalogSellerOwnershipTests
{
    [Fact]
    public async Task SellerMustChooseAStoreWhenCreatingProduct()
    {
        ProductService service = CreateService(new FakeStoreRepository());

        Result<ProductResponse> result = await service.CreateAsync(
            ProductRequest(storeId: null),
            new ProductAccessContext(Guid.NewGuid(), IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.StoreRequired, result.Error?.Code);
    }

    [Fact]
    public async Task SellerCannotCreateProductForAnotherOwnersStore()
    {
        Guid storeId = Guid.NewGuid();
        FakeStoreRepository stores = new(new Store(storeId, Guid.NewGuid(), "Other Store", "other-store"));
        ProductService service = CreateService(stores);

        Result<ProductResponse> result = await service.CreateAsync(
            ProductRequest(storeId),
            new ProductAccessContext(Guid.NewGuid(), IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.StoreAccessDenied, result.Error?.Code);
    }

    [Fact]
    public async Task SellerCanCreateProductForOwnedStore()
    {
        Guid sellerId = Guid.NewGuid();
        Guid storeId = Guid.NewGuid();
        FakeStoreRepository stores = new(new Store(storeId, sellerId, "My Store", "my-store"));
        ProductService service = CreateService(stores);

        Result<ProductResponse> result = await service.CreateAsync(
            ProductRequest(storeId),
            new ProductAccessContext(sellerId, IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(storeId, result.Value?.StoreId);
    }

    [Fact]
    public async Task AdminCanCreateLegacyPlatformProductWithoutStore()
    {
        ProductService service = CreateService(new FakeStoreRepository());

        Result<ProductResponse> result = await service.CreateAsync(
            ProductRequest(storeId: null),
            new ProductAccessContext(UserId: null, IsAdmin: true),
            "en",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value?.StoreId);
    }

    [Fact]
    public async Task SellerCannotUpdateLegacyPlatformProduct()
    {
        Product platformProduct = new(
            Guid.NewGuid(),
            "PLATFORM-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            storeId: null,
            10m,
            "TRY",
            ProductStatus.Active);
        FakeProductRepository products = new(platformProduct);
        ProductService service = new(
            products,
            new ProductStoreAccessValidator(new FakeStoreRepository()),
            new ProductReferenceValidator(products),
            new ProductImageAttacher(new AlwaysValidImageService()));

        Result<ProductResponse> result = await service.UpdateAsync(
            platformProduct.Id,
            new UpdateProductRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                20m,
                "TRY",
                ProductStatus.Active,
                [new ProductTranslationInput("en", "Updated", "Description")]),
            new ProductAccessContext(Guid.NewGuid(), IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.StoreAccessDenied, result.Error?.Code);
    }

    [Fact]
    public async Task SellerCannotUpdateAnotherOwnersProduct()
    {
        Guid storeId = Guid.NewGuid();
        Store otherStore = new(storeId, Guid.NewGuid(), "Other Store", "other-store");
        Product product = new(
            Guid.NewGuid(),
            "OTHER-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            storeId,
            10m,
            "TRY",
            ProductStatus.Active);
        FakeProductRepository products = new(product);
        ProductService service = new(
            products,
            new ProductStoreAccessValidator(new FakeStoreRepository(otherStore)),
            new ProductReferenceValidator(products),
            new ProductImageAttacher(new AlwaysValidImageService()));

        Result<ProductResponse> result = await service.UpdateAsync(
            product.Id,
            new UpdateProductRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                20m,
                "TRY",
                ProductStatus.Active,
                [new ProductTranslationInput("en", "Updated", "Description")]),
            new ProductAccessContext(Guid.NewGuid(), IsAdmin: false),
            "en",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.StoreAccessDenied, result.Error?.Code);
    }

    [Fact]
    public async Task StoreOwnerComesFromTrustedArgumentAndIsNotPartOfRequest()
    {
        Guid sellerId = Guid.NewGuid();
        FakeStoreRepository stores = new();
        StoreService service = new(stores);

        Result<StoreResponse> result = await service.CreateAsync(
            sellerId,
            new CreateStoreRequest("Seller Store", "seller-store"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(sellerId, stores.Added?.OwnerUserId);
        Assert.DoesNotContain(typeof(CreateStoreRequest).GetProperties(), property => property.Name == "OwnerUserId");
    }

    private static ProductService CreateService(FakeStoreRepository stores)
    {
        FakeProductRepository products = new();
        return new ProductService(
            products,
            new ProductStoreAccessValidator(stores),
            new ProductReferenceValidator(products),
            new ProductImageAttacher(new AlwaysValidImageService()));
    }

    private static CreateProductRequest ProductRequest(Guid? storeId) => new(
        "SKU-1",
        Guid.NewGuid(),
        Guid.NewGuid(),
        storeId,
        10m,
        "TRY",
        ProductStatus.Active,
        [new ProductTranslationInput("en", "Product", "Description")],
        []);

}
