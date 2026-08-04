namespace ECommerce.Catalog.Application.References;

public sealed record ManagedCatalogBrandResponse(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive);
