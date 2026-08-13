using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class ProductImageInvariantTests
{
    [Fact]
    public void AddImageRejectsImagesBeyondAggregateLimitWithoutChangingState()
    {
        Product product = CreateProduct();
        for (int index = 0; index < Product.MaximumImageCount; index++)
        {
            Assert.Equal(
                ProductImageMutationResult.Applied,
                product.AddImage(CreateImage(product.Id, index)));
        }

        DateTimeOffset updatedAt = product.UpdatedAt;
        ProductImageMutationResult result = product.AddImage(
            CreateImage(product.Id, Product.MaximumImageCount));

        Assert.Equal(ProductImageMutationResult.LimitExceeded, result);
        Assert.Equal(Product.MaximumImageCount, product.Images.Count);
        Assert.Equal(updatedAt, product.UpdatedAt);
    }

    [Fact]
    public void AddImageRejectsImageOwnedByAnotherProductWithoutChangingState()
    {
        Product product = CreateProduct();
        DateTimeOffset updatedAt = product.UpdatedAt;

        ProductImageMutationResult result = product.AddImage(
            CreateImage(Guid.NewGuid(), 0));

        Assert.Equal(ProductImageMutationResult.InvalidImage, result);
        Assert.Empty(product.Images);
        Assert.Equal(updatedAt, product.UpdatedAt);
    }

    [Fact]
    public void AddImageRejectsDuplicateIdentityWithoutChangingState()
    {
        Product product = CreateProduct();
        ProductImage image = CreateImage(product.Id, 0);
        Assert.Equal(ProductImageMutationResult.Applied, product.AddImage(image));
        DateTimeOffset updatedAt = product.UpdatedAt;

        ProductImageMutationResult result = product.AddImage(image);

        Assert.Equal(ProductImageMutationResult.DuplicateImage, result);
        Assert.Single(product.Images);
        Assert.Equal(updatedAt, product.UpdatedAt);
    }

    private static Product CreateProduct()
    {
        return new Product(
            Guid.NewGuid(),
            "IMAGE-INVARIANT",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "TRY",
            ProductStatus.Draft);
    }

    private static ProductImage CreateImage(Guid productId, int sortOrder)
    {
        return new ProductImage(
            Guid.NewGuid(),
            productId,
            $"image-{sortOrder}",
            $"http://example.com/image-{sortOrder}.png",
            $"https://example.com/image-{sortOrder}.png",
            100,
            100,
            "png",
            sortOrder,
            sortOrder == 0);
    }
}
