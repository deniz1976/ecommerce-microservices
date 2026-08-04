namespace ECommerce.Catalog.Application.References;

public sealed record UpdateCatalogCategoryRequest(
    string Slug,
    string EnglishName,
    string TurkishName,
    bool IsActive);
