namespace ECommerce.Inventory.Domain;

public enum StockMovementType
{
    StockInitialized = 1,
    StockIncreased = 2,
    StockDecreased = 3,
    StockReserved = 4,
    StockReleased = 5,
    AuditBaseline = 6,
    StockShipped = 7
}
