namespace ECommerce.Catalog.Application.Stores;

public sealed record ManagedStoreListCriteria(
    int PageNumber,
    int PageSize,
    string? Search,
    Guid? OwnerUserId,
    string? SortBy,
    bool SortDescending);
