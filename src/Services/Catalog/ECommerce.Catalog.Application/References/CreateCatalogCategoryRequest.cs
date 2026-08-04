namespace ECommerce.Catalog.Application.References;

public sealed record CreateCatalogCategoryRequest(
    string Slug,
    string EnglishName,
    string TurkishName);
