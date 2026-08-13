namespace ECommerce.Inventory.Application.Inventory;

public sealed record StockMovementResponse(
    Guid Id,
    Guid ProductId,
    StockMovementKind Type,
    int Quantity,
    int QuantityOnHandBefore,
    int QuantityOnHandAfter,
    int ReservedQuantityBefore,
    int ReservedQuantityAfter,
    Guid? OrderId,
    DateTimeOffset OccurredAt);
