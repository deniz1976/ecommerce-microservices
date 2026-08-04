namespace ECommerce.Inventory.Application.Inventory;

public sealed record ManagedInventoryListCriteria(
    int PageNumber,
    int PageSize,
    Guid? ProductId,
    int? MaximumAvailableQuantity,
    string? SortBy,
    bool SortDescending);
