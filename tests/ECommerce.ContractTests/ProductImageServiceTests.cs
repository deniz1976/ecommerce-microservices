using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace ECommerce.ContractTests;

public sealed class ProductImageServiceTests
{
    [Fact]
    public async Task UploadAsync_rejects_content_type_that_does_not_match_file_signature()
    {
        (Product product, ProductImageUploadService service, _, TestProductImageStorage storage, _) = CreateServices();
        await using MemoryStream content = new("not-an-image"u8.ToArray());

        Result<ProductImageResponse> result = await service.UploadAsync(
            product.Id,
            new ProductImageUpload(content, "fake.png", "image/png", content.Length),
            new ProductAccessContext(TestSellerId, IsAdmin: false),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("INVALID_PRODUCT_IMAGE", result.Error?.Code);
        Assert.Equal(0, storage.UploadCount);
    }

    [Fact]
    public async Task UploadAsync_uses_provider_metadata_and_marks_first_image_as_main()
    {
        (Product product, ProductImageUploadService service, _, TestProductImageStorage storage, _) = CreateServices();
        await using MemoryStream content = new(
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x00
        ]);

        Result<ProductImageResponse> result = await service.UploadAsync(
            product.Id,
            new ProductImageUpload(content, "product.png", "image/png", content.Length),
            new ProductAccessContext(TestSellerId, IsAdmin: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value?.IsMain);
        Assert.StartsWith("ecommerce/products/", result.Value?.PublicId);
        Assert.Equal(1, storage.UploadCount);
    }

    [Fact]
    public async Task UploadAsync_rejects_aggregate_image_limit_before_provider_upload()
    {
        (Product product, ProductImageUploadService service, _, TestProductImageStorage storage, _) = CreateServices();
        for (int index = 0; index < Product.MaximumImageCount; index++)
        {
            ProductImage image = new(
                Guid.NewGuid(),
                product.Id,
                $"existing-{index}",
                $"http://example.com/existing-{index}.png",
                $"https://example.com/existing-{index}.png",
                100,
                100,
                "png",
                index,
                index == 0);
            Assert.Equal(ProductImageMutationResult.Applied, product.AddImage(image));
        }

        await using MemoryStream content = new(
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x00
        ]);

        Result<ProductImageResponse> result = await service.UploadAsync(
            product.Id,
            new ProductImageUpload(content, "ninth.png", "image/png", content.Length),
            new ProductAccessContext(TestSellerId, IsAdmin: false),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.ProductImageLimitExceeded, result.Error?.Code);
        Assert.Equal(0, storage.UploadCount);
    }

    [Fact]
    public async Task DeleteAsync_removes_metadata_and_provider_asset()
    {
        (
            Product product,
            _,
            ProductImageDeletionService service,
            TestProductImageStorage storage,
            FakeProductImageDeletionQueue deletionQueue) = CreateServices();
        ProductImage image = new(
            Guid.NewGuid(),
            product.Id,
            "existing-public-id",
            "http://example.com/image.png",
            "https://example.com/image.png",
            100,
            100,
            "png",
            0,
            true);
        product.AddImage(image);

        Result<ProductImageResponse> result = await service.DeleteAsync(
            product.Id,
            image.Id,
            new ProductAccessContext(TestSellerId, IsAdmin: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(product.Images);
        Assert.Contains("existing-public-id", storage.DeletedPublicIds);
        Assert.Empty(deletionQueue.PendingPublicIds);
    }

    [Fact]
    public async Task DeleteAsync_keeps_durable_cleanup_when_provider_delete_fails()
    {
        (
            Product product,
            _,
            ProductImageDeletionService service,
            TestProductImageStorage storage,
            FakeProductImageDeletionQueue deletionQueue) = CreateServices();
        storage.DeleteSucceeds = false;
        ProductImage image = new(
            Guid.NewGuid(),
            product.Id,
            "deferred-public-id",
            "http://example.com/image.png",
            "https://example.com/image.png",
            100,
            100,
            "png",
            0,
            true);
        product.AddImage(image);

        Result<ProductImageResponse> result = await service.DeleteAsync(
            product.Id,
            image.Id,
            new ProductAccessContext(TestSellerId, IsAdmin: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(product.Images);
        Assert.Contains("deferred-public-id", deletionQueue.PendingPublicIds);
    }

    private static readonly Guid TestSellerId = Guid.NewGuid();

    private static (
        Product Product,
        ProductImageUploadService UploadService,
        ProductImageDeletionService DeletionService,
        TestProductImageStorage Storage,
        FakeProductImageDeletionQueue DeletionQueue) CreateServices()
    {
        Guid storeId = Guid.NewGuid();
        Product product = new(
            Guid.NewGuid(),
            "IMAGE-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            storeId,
            10m,
            "USD",
            ProductStatus.Draft);
        FakeProductRepository products = new(product);
        FakeStoreRepository stores = new(new Store(storeId, TestSellerId, "Image Store", "image-store"));
        TestProductImageStorage storage = new();
        FakeProductImageDeletionQueue deletionQueue = new();
        ProductImageAccessService accessService = new(
            products,
            new ProductStoreAccessValidator(stores));
        ProductImageUploadService uploadService = new(
            products,
            accessService,
            new ProductImageUploadValidator(),
            storage);
        ProductImageDeletionService deletionService = new(
            products,
            accessService,
            storage,
            deletionQueue,
            NullLogger<ProductImageDeletionService>.Instance);
        return (product, uploadService, deletionService, storage, deletionQueue);
    }
}
