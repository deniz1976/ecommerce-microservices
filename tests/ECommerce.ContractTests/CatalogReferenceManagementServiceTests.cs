using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class CatalogReferenceManagementServiceTests
{
    [Fact]
    public async Task AdminCategoryCreationNormalizesSlugAndPersistsBothLanguages()
    {
        FakeCatalogReferenceWriter writer = new();
        CatalogReferenceManagementService service = CreateService(writer);

        var result = await service.CreateCategoryAsync(
            new CreateCatalogCategoryRequest(
                "  elektronik  ",
                "Electronics",
                "Elektronik"),
            "tr",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Elektronik", result.Value!.Name);
        Assert.Equal("elektronik", result.Value.Slug);
        Assert.NotNull(writer.AddedCategory);
        Assert.Collection(
            writer.AddedCategory.Translations.OrderBy(item => item.LanguageCode),
            item =>
            {
                Assert.Equal("en", item.LanguageCode);
                Assert.Equal("Electronics", item.Name);
            },
            item =>
            {
                Assert.Equal("tr", item.LanguageCode);
                Assert.Equal("Elektronik", item.Name);
            });
    }

    [Fact]
    public async Task DuplicateCategorySlugUsesStableConflictCode()
    {
        Category existing = new(Guid.NewGuid(), "electronics", true);
        existing.SetTranslation("en", "Electronics");
        existing.SetTranslation("tr", "Elektronik");
        FakeCatalogReferenceWriter writer = new([existing]);
        CatalogReferenceManagementService service = CreateService(writer);

        var result = await service.CreateCategoryAsync(
            new CreateCatalogCategoryRequest(
                "electronics",
                "Electronics",
                "Elektronik"),
            "en",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrorCodes.CategorySlugConflict, result.Error!.Code);
        Assert.Null(writer.AddedCategory);
    }

    [Fact]
    public async Task AdminBrandCreationPersistsAnActiveReference()
    {
        FakeCatalogReferenceWriter writer = new();
        CatalogReferenceManagementService service = CreateService(writer);

        var result = await service.CreateBrandAsync(
            new CreateCatalogBrandRequest("Örnek Marka", "ornek-marka"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Örnek Marka", result.Value!.Name);
        Assert.Equal("ornek-marka", result.Value.Slug);
        Assert.NotNull(writer.AddedBrand);
        Assert.True(writer.AddedBrand.IsActive);
    }

    [Fact]
    public async Task CategoryCanBeUpdatedAndDeactivatedWithoutChangingItsIdentity()
    {
        Category category = new(Guid.NewGuid(), "electronics", true);
        category.SetTranslation("en", "Electronics");
        category.SetTranslation("tr", "Elektronik");
        FakeCatalogReferenceWriter writer = new([category]);
        CatalogReferenceManagementService service = CreateService(writer);

        var result = await service.UpdateCategoryAsync(
            category.Id,
            new UpdateCatalogCategoryRequest(
                "consumer-electronics",
                "Consumer Electronics",
                "Tüketici Elektroniği",
                IsActive: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(category.Id, result.Value!.Id);
        Assert.Equal("consumer-electronics", result.Value.Slug);
        Assert.Equal("Tüketici Elektroniği", result.Value.TurkishName);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public async Task BrandCanBeUpdatedAndReactivated()
    {
        Brand brand = new(Guid.NewGuid(), "Eski Marka", "eski-marka", false);
        FakeCatalogReferenceWriter writer = new(brands: [brand]);
        CatalogReferenceManagementService service = CreateService(writer);

        var result = await service.UpdateBrandAsync(
            brand.Id,
            new UpdateCatalogBrandRequest(
                "Yeni Marka",
                "yeni-marka",
                IsActive: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Yeni Marka", result.Value!.Name);
        Assert.Equal("yeni-marka", result.Value.Slug);
        Assert.True(result.Value.IsActive);
    }

    private static CatalogReferenceManagementService CreateService(
        FakeCatalogReferenceWriter writer) =>
        new(writer, writer, writer, writer);
}
