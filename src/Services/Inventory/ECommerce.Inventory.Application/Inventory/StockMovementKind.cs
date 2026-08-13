namespace ECommerce.Inventory.Application.Inventory;

public enum StockMovementKind
{
    StockInitialized = 1,
    StockIncreased = 2,
    StockDecreased = 3,
    StockReserved = 4,
    StockReleased = 5,
    AuditBaseline = 6
}
