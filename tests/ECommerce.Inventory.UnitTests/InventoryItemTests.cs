using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.UnitTests;

public sealed class InventoryItemTests
{
    [Fact]
    public void CanReserve_returns_true_when_available_quantity_is_enough()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);

        Assert.True(item.CanReserve(4));
    }

    [Fact]
    public void Reserve_increases_reserved_quantity_and_records_movement()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.DequeuePendingMovements();
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();

        item.Reserve(3, orderId, reservationId);

        Assert.Equal(3, item.ReservedQuantity);
        Assert.Equal(7, item.AvailableQuantity);
        StockMovement movement = Assert.Single(item.PendingMovements);
        Assert.Equal(StockMovementType.StockReserved, movement.Type);
        Assert.Equal(orderId, movement.OrderId);
        Assert.Equal(reservationId, movement.ReservationId);
        Assert.Equal(0, movement.ReservedQuantityBefore);
        Assert.Equal(3, movement.ReservedQuantityAfter);
    }

    [Fact]
    public void Release_records_only_the_quantity_that_was_reserved()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(2, Guid.NewGuid(), Guid.NewGuid());
        item.DequeuePendingMovements();
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();

        item.Release(5, orderId, reservationId);

        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(10, item.AvailableQuantity);
        StockMovement movement = Assert.Single(item.PendingMovements);
        Assert.Equal(StockMovementType.StockReleased, movement.Type);
        Assert.Equal(2, movement.Quantity);
        Assert.Equal(orderId, movement.OrderId);
        Assert.Equal(reservationId, movement.ReservationId);
    }

    [Fact]
    public void SetQuantityOnHand_updates_total_stock_without_changing_reserved_quantity()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(4, Guid.NewGuid(), Guid.NewGuid());

        item.SetQuantityOnHand(12);

        Assert.Equal(12, item.QuantityOnHand);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.Equal(8, item.AvailableQuantity);
    }

    [Fact]
    public void SetQuantityOnHand_rejects_quantity_below_reserved_stock()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(4, Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => item.SetQuantityOnHand(3));
    }

    [Fact]
    public void Constructor_records_initial_stock_movement_in_utc()
    {
        Guid productId = Guid.NewGuid();

        InventoryItem item = new(productId, 10);

        StockMovement movement = Assert.Single(item.PendingMovements);
        Assert.Equal(StockMovementType.StockInitialized, movement.Type);
        Assert.Equal(productId, movement.ProductId);
        Assert.Equal(10, movement.Quantity);
        Assert.Equal(0, movement.QuantityOnHandBefore);
        Assert.Equal(10, movement.QuantityOnHandAfter);
        Assert.Equal(TimeSpan.Zero, movement.OccurredAt.Offset);
    }

    [Fact]
    public void SetQuantityOnHand_records_increase_and_decrease_movements()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.DequeuePendingMovements();

        item.SetQuantityOnHand(15);
        StockMovement increase = Assert.Single(item.DequeuePendingMovements());
        item.SetQuantityOnHand(12);
        StockMovement decrease = Assert.Single(item.DequeuePendingMovements());

        Assert.Equal(StockMovementType.StockIncreased, increase.Type);
        Assert.Equal(5, increase.Quantity);
        Assert.Equal(StockMovementType.StockDecreased, decrease.Type);
        Assert.Equal(3, decrease.Quantity);
    }

    [Fact]
    public void Setting_same_quantity_does_not_record_movement()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.DequeuePendingMovements();

        item.SetQuantityOnHand(10);

        Assert.Empty(item.PendingMovements);
    }
}
