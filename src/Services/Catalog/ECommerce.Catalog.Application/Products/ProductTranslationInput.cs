namespace ECommerce.Catalog.Application.Products;

public sealed record ProductTranslationInput(
    string LanguageCode,
    string Name,
    string Description);
