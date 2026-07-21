using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed record ProductListQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    Guid? CategoryId,
    Guid? BrandId,
    Guid? StoreId,
    ProductStatus? Status,
    string? SortBy,
    bool SortDescending);
