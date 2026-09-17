using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class ProductMapperImageOrderTests
{
    [Fact]
    public void MainImageIsReturnedFirst()
    {
        Guid productId = Guid.NewGuid();
        Product product = new(
            productId,
            "SKU-IMAGE-ORDER",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "USD",
            ProductStatus.Active);
        product.SetTranslation("en", "name", "description");
        product.AddImage(CreateImage(productId, 0));
        Guid secondImageId = Guid.NewGuid();
        product.AddImage(CreateImage(productId, 1, secondImageId));
        product.SetMainImage(secondImageId);

        ProductResponse response = product.ToResponse("en");

        ProductImageResponse firstImage = response.Images.First();
        Assert.Equal(secondImageId, firstImage.Id);
        Assert.True(firstImage.IsMain);
    }

    private static ProductImage CreateImage(Guid productId, int sortOrder, Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            productId,
            $"ecommerce/products/sample-{sortOrder}",
            $"http://images.example/sample-{sortOrder}",
            $"https://images.example/sample-{sortOrder}",
            64,
            64,
            "png",
            sortOrder,
            sortOrder == 0);
}
