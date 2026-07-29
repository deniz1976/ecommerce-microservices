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
                .Select(x => x.ToResponse())
                .ToArray());
    }

    public static ProductImageResponse ToResponse(this ProductImage image)
    {
        return new ProductImageResponse(
            image.Id,
            image.PublicId,
            image.Url,
            image.SecureUrl,
            image.Width,
            image.Height,
            image.Format,
            image.SortOrder,
            image.IsMain);
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
