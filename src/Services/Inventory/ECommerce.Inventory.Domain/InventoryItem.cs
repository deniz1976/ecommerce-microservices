namespace ECommerce.Inventory.Domain;

public sealed class InventoryItem
{
    private readonly List<StockMovement> pendingMovements = [];

    private InventoryItem()
    {
    }

    public InventoryItem(Guid productId, int quantityOnHand)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantityOnHand);
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        ReservedQuantity = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            StockMovementType.StockInitialized,
            quantityOnHand,
            0,
            quantityOnHand,
            0,
            0,
            null,
            null);
    }

    public Guid ProductId { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public DateTimeOffset UpdatedAt { get; private set; }

    public long ConcurrencyVersion { get; private set; }

    public IReadOnlyCollection<StockMovement> PendingMovements => pendingMovements;

    public bool CanReserve(int quantity)
    {
        return quantity > 0 && AvailableQuantity >= quantity;
    }

    public InventoryReservationMutationResult Reserve(
        int quantity,
        Guid orderId,
        Guid reservationId)
    {
        if (quantity <= 0)
        {
            return InventoryReservationMutationResult.InvalidQuantity;
        }

        if (AvailableQuantity < quantity)
        {
            return InventoryReservationMutationResult.InsufficientStock;
        }

        int reservedBefore = ReservedQuantity;
        ReservedQuantity += quantity;
        ConcurrencyVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            StockMovementType.StockReserved,
            quantity,
            QuantityOnHand,
            QuantityOnHand,
            reservedBefore,
            ReservedQuantity,
            orderId,
            reservationId);
        return InventoryReservationMutationResult.Applied;
    }

    public void Release(int quantity, Guid orderId, Guid reservationId)
    {
        int reservedBefore = ReservedQuantity;
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        int releasedQuantity = reservedBefore - ReservedQuantity;
        if (releasedQuantity == 0)
        {
            return;
        }

        ConcurrencyVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            StockMovementType.StockReleased,
            releasedQuantity,
            QuantityOnHand,
            QuantityOnHand,
            reservedBefore,
            ReservedQuantity,
            orderId,
            reservationId);
    }

    public void Ship(int quantity, Guid orderId, Guid reservationId)
    {
        int reservedBefore = ReservedQuantity;
        int shippedQuantity = Math.Min(Math.Max(0, quantity), reservedBefore);
        if (shippedQuantity == 0)
        {
            return;
        }

        int quantityOnHandBefore = QuantityOnHand;
        ReservedQuantity -= shippedQuantity;
        QuantityOnHand -= shippedQuantity;
        ConcurrencyVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            StockMovementType.StockShipped,
            shippedQuantity,
            quantityOnHandBefore,
            QuantityOnHand,
            reservedBefore,
            ReservedQuantity,
            orderId,
            reservationId);
    }

    public void IncreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        int quantityBefore = QuantityOnHand;
        QuantityOnHand += quantity;
        ConcurrencyVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            StockMovementType.StockIncreased,
            quantity,
            quantityBefore,
            QuantityOnHand,
            ReservedQuantity,
            ReservedQuantity,
            null,
            null);
    }

    public void SetQuantityOnHand(int quantityOnHand)
    {
        if (quantityOnHand < ReservedQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityOnHand),
                "Quantity on hand cannot be lower than the reserved quantity.");
        }

        int quantityBefore = QuantityOnHand;
        if (quantityBefore == quantityOnHand)
        {
            return;
        }

        QuantityOnHand = quantityOnHand;
        ConcurrencyVersion++;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecordMovement(
            quantityOnHand > quantityBefore
                ? StockMovementType.StockIncreased
                : StockMovementType.StockDecreased,
            Math.Abs(quantityOnHand - quantityBefore),
            quantityBefore,
            QuantityOnHand,
            ReservedQuantity,
            ReservedQuantity,
            null,
            null);
    }

    public IReadOnlyCollection<StockMovement> DequeuePendingMovements()
    {
        StockMovement[] movements = pendingMovements.ToArray();
        pendingMovements.Clear();
        return movements;
    }

    private void RecordMovement(
        StockMovementType type,
        int quantity,
        int quantityOnHandBefore,
        int quantityOnHandAfter,
        int reservedQuantityBefore,
        int reservedQuantityAfter,
        Guid? orderId,
        Guid? reservationId)
    {
        pendingMovements.Add(new StockMovement(
            ProductId,
            type,
            quantity,
            quantityOnHandBefore,
            quantityOnHandAfter,
            reservedQuantityBefore,
            reservedQuantityAfter,
            orderId,
            reservationId));
    }
}
