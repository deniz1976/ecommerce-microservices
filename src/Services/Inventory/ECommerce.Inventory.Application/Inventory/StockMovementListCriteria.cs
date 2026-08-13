namespace ECommerce.Inventory.Application.Inventory;

public sealed record StockMovementListCriteria(
    Guid ProductId,
    int PageNumber,
    int PageSize,
    Guid? OrderId,
    StockMovementKind? Type);
