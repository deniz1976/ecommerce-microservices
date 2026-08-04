namespace ECommerce.Catalog.Application.References;

public sealed record ManagedCatalogBrandListCriteria(
    int PageNumber,
    int PageSize,
    string? Search,
    bool? IsActive,
    string? SortBy,
    bool SortDescending);
