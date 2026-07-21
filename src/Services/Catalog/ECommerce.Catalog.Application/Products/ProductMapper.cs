using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public static class ProductMapper
{
    public static ProductResponse ToResponse(this Product product, string culture)
    {
        ProductTranslation? translation = SelectTranslation(product.Translations, culture);
        CategoryTranslation? categoryTranslation = product.Category is null ? null : SelectTranslation(product.Category.Translations, culture);

        return new ProductResponse(
            product.Id,
            product.Sku,
            translation?.Name ?? string.Empty,
            translation?.Description ?? string.Empty,
            product.CategoryId,
            categoryTranslation?.Name,
            product.BrandId,
            product.Brand?.Name,
            product.StoreId,
            product.Price,
            product.Currency,
            product.Status,
            product.Images
                .OrderBy(x => x.SortOrder)
                .Select(x => new ProductImageResponse(x.Id, x.PublicId, x.Url, x.SecureUrl, x.Width, x.Height, x.Format, x.SortOrder, x.IsMain))
                .ToArray());
    }

    private static ProductTranslation? SelectTranslation(IEnumerable<ProductTranslation> translations, string culture)
    {
        return translations.FirstOrDefault(x => x.LanguageCode == culture)
            ?? translations.FirstOrDefault(x => x.LanguageCode == "en");
    }

    private static CategoryTranslation? SelectTranslation(IEnumerable<CategoryTranslation> translations, string culture)
    {
        return translations.FirstOrDefault(x => x.LanguageCode == culture)
            ?? translations.FirstOrDefault(x => x.LanguageCode == "en");
    }
}
