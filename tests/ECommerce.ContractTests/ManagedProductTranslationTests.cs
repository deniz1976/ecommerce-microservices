using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class ManagedProductTranslationTests
{
    [Fact]
    public async Task ManagementReadReturnsEveryTranslationInsteadOfTheResolvedCulture()
    {
        Product product = new(
            Guid.NewGuid(),
            "TRANSLATION-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            storeId: null,
            10m,
            "TRY",
            ProductStatus.Draft);
        product.SetTranslation("en", "English name", "English description");
        product.SetTranslation("tr", "Türkçe ad", "Türkçe açıklama");
        FakeProductRepository products = new(product);
        ManagedProductQueryService service = new(
            products,
            products,
            new ProductStoreAccessValidator(new FakeStoreRepository()));

        Result<ManagedProductResponse> result = await service.GetByIdAsync(
            product.Id,
            new ProductAccessContext(UserId: null, IsAdmin: true),
            "en",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("English name", result.Value!.Name);
        Assert.Equal(2, result.Value.Translations.Count);
        ProductTranslationResponse turkish = Assert.Single(
            result.Value.Translations,
            translation => translation.LanguageCode == "tr");
        Assert.Equal("Türkçe ad", turkish.Name);
        Assert.Equal("Türkçe açıklama", turkish.Description);
    }

    [Fact]
    public async Task ManagementReadDoesNotInventATranslationForAMissingLanguage()
    {
        Product product = new(
            Guid.NewGuid(),
            "TRANSLATION-2",
            Guid.NewGuid(),
            Guid.NewGuid(),
            storeId: null,
            10m,
            "TRY",
            ProductStatus.Draft);
        product.SetTranslation("en", "English name", "English description");
        FakeProductRepository products = new(product);
        ManagedProductQueryService service = new(
            products,
            products,
            new ProductStoreAccessValidator(new FakeStoreRepository()));

        Result<ManagedProductResponse> result = await service.GetByIdAsync(
            product.Id,
            new ProductAccessContext(UserId: null, IsAdmin: true),
            "tr",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("English name", result.Value!.Name);
        ProductTranslationResponse only = Assert.Single(result.Value.Translations);
        Assert.Equal("en", only.LanguageCode);
    }
}
