namespace ECommerce.Catalog.Application.References;

public sealed record UpdateCatalogBrandRequest(
    string Name,
    string Slug,
    bool IsActive);
