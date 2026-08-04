namespace ECommerce.Catalog.Application.References;

public sealed record ManagedCatalogCategoryResponse(
    Guid Id,
    string EnglishName,
    string TurkishName,
    string Slug,
    bool IsActive);
