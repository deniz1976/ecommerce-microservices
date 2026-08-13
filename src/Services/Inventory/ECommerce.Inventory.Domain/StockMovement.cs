namespace ECommerce.Inventory.Domain;

public sealed class StockMovement
{
    private StockMovement()
    {
    }

    public StockMovement(
        Guid productId,
        StockMovementType type,
        int quantity,
        int quantityOnHandBefore,
        int quantityOnHandAfter,
        int reservedQuantityBefore,
        int reservedQuantityAfter,
        Guid? orderId,
        Guid? reservationId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        QuantityOnHandBefore = quantityOnHandBefore;
        QuantityOnHandAfter = quantityOnHandAfter;
        ReservedQuantityBefore = reservedQuantityBefore;
        ReservedQuantityAfter = reservedQuantityAfter;
        OrderId = orderId;
        ReservationId = reservationId;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public StockMovementType Type { get; private set; }

    public int Quantity { get; private set; }

    public int QuantityOnHandBefore { get; private set; }

    public int QuantityOnHandAfter { get; private set; }

    public int ReservedQuantityBefore { get; private set; }

    public int ReservedQuantityAfter { get; private set; }

    public Guid? OrderId { get; private set; }

    public Guid? ReservationId { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
