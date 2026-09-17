using ECommerce.Catalog.Domain;
using ECommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ECommerce.ContractTests;

public sealed class ProductImageConfigurationTests
{
    [Fact]
    public void ProductImageIdUsesDomainGeneratedValue()
    {
        using CatalogDbContext dbContext = CreateDbContext();

        IProperty idProperty = dbContext.Model
            .FindEntityType(typeof(ProductImage))!
            .FindProperty(nameof(ProductImage.Id))!;

        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void ImageAddedToTrackedProductIsInserted()
    {
        using CatalogDbContext dbContext = CreateDbContext();
        Guid productId = Guid.NewGuid();
        Product product = new(
            productId,
            "SKU-IMAGE-STATE",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "USD",
            ProductStatus.Active);
        dbContext.Attach(product);

        ProductImage image = new(
            Guid.NewGuid(),
            productId,
            "ecommerce/products/sample",
            "http://images.example/sample",
            "https://images.example/sample",
            64,
            64,
            "png",
            0,
            true);
        Assert.Equal(ProductImageMutationResult.Applied, product.AddImage(image));

        dbContext.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Added, dbContext.Entry(image).State);
    }

    private static CatalogDbContext CreateDbContext()
    {
        DbContextOptions<CatalogDbContext> options =
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql("Host=localhost;Database=model_check")
                .Options;
        return new CatalogDbContext(options);
    }
}
