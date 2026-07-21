using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

public sealed class CatalogReferenceServiceTests
{
    [Fact]
    public async Task CategoriesUseRequestedTranslationAndFallBackToEnglish()
    {
        Category translated = new(Guid.NewGuid(), "electronics", true);
        translated.SetTranslation("en", "Electronics");
        translated.SetTranslation("tr", "Elektronik");
        Category fallback = new(Guid.NewGuid(), "home", true);
        fallback.SetTranslation("en", "Home");
        CatalogReferenceService service = new(new FakeCatalogReferenceRepository
        {
            Categories = [translated, fallback]
        });

        IReadOnlyCollection<CatalogCategoryResponse> result = await service.GetCategoriesAsync("tr", CancellationToken.None);

        Assert.Collection(
            result,
            item => Assert.Equal("Elektronik", item.Name),
            item => Assert.Equal("Home", item.Name));
    }

    [Fact]
    public async Task BrandsAreMappedWithoutExposingDomainEntities()
    {
        Brand brand = new(Guid.NewGuid(), "Acme", "acme", true);
        CatalogReferenceService service = new(new FakeCatalogReferenceRepository { Brands = [brand] });

        CatalogBrandResponse result = Assert.Single(await service.GetBrandsAsync(CancellationToken.None));

        Assert.Equal(brand.Id, result.Id);
        Assert.Equal("Acme", result.Name);
        Assert.Equal("acme", result.Slug);
    }
}
