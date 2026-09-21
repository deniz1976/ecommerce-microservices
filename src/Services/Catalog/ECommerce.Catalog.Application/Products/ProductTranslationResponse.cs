namespace ECommerce.Catalog.Application.Products;

public sealed record ProductTranslationResponse(
    string LanguageCode,
    string Name,
    string Description);
