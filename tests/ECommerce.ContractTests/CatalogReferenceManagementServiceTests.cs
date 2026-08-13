using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Commands.CreateCatalogBrand;
using ECommerce.Catalog.Application.Commands.CreateCatalogCategory;
using ECommerce.Catalog.Application.Commands.UpdateCatalogBrand;
using ECommerce.Catalog.Application.Commands.UpdateCatalogCategory;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class CatalogReferenceManagementServiceTests
{
    [Theory]
    [InlineData(typeof(CreateCatalogCategoryCommandHandler), typeof(CatalogCategoryManagementService))]
    [InlineData(typeof(UpdateCatalogCategoryCommandHandler), typeof(CatalogCategoryManagementService))]
    [InlineData(typeof(CreateCatalogBrandCommandHandler), typeof(CatalogBrandManagementService))]
    [InlineData(typeof(UpdateCatalogBrandCommandHandler), typeof(CatalogBrandManagementService))]
    public void ReferenceHandlersDependOnTheirFocusedManagementService(
        Type handlerType,
        Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public async Task AdminCategoryCreationNormalizesSlugAndPersistsBothLanguages()
    {
        FakeCatalogReferenceWriter writer = new();
        CatalogCategoryManagementService service = CreateCategoryService(writer);

        var result = await service.CreateAsync(
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
        CatalogCategoryManagementService service = CreateCategoryService(writer);

        var result = await service.CreateAsync(
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
        CatalogBrandManagementService service = CreateBrandService(writer);

        var result = await service.CreateAsync(
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
        CatalogCategoryManagementService service = CreateCategoryService(writer);

        var result = await service.UpdateAsync(
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
        CatalogBrandManagementService service = CreateBrandService(writer);

        var result = await service.UpdateAsync(
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

    private static CatalogCategoryManagementService CreateCategoryService(
        FakeCatalogReferenceWriter writer) =>
        new(writer, writer, writer);

    private static CatalogBrandManagementService CreateBrandService(
        FakeCatalogReferenceWriter writer) =>
        new(writer, writer, writer);
}
