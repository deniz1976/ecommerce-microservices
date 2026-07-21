using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Images;
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
        ProductService service = new(
            new FakeProductRepository(platformProduct),
            new AlwaysValidImageService(),
            new FakeStoreRepository());

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
        ProductService service = new(
            new FakeProductRepository(product),
            new AlwaysValidImageService(),
            new FakeStoreRepository(otherStore));

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
        return new ProductService(new FakeProductRepository(), new AlwaysValidImageService(), stores);
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

    private sealed class FakeProductRepository : IProductRepository
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

    private sealed class FakeStoreRepository : IStoreRepository
    {
        private readonly Dictionary<Guid, Store> stores;

        public FakeStoreRepository(params Store[] stores)
        {
            this.stores = stores.ToDictionary(x => x.Id);
        }

        public Store? Added { get; private set; }

        public Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(stores.GetValueOrDefault(id));

        public Task<IReadOnlyCollection<Store>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Store>>(stores.Values.Where(x => x.OwnerUserId == ownerUserId).ToArray());

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
            Task.FromResult(stores.Values.Any(x => x.Slug == slug));

        public void Add(Store store)
        {
            Added = store;
            stores.Add(store.Id, store);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class AlwaysValidImageService : ICloudImageService
    {
        public Task<bool> ImageExistsAsync(string publicId, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
