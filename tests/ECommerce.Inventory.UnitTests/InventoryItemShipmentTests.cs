using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.UnitTests;

public sealed class InventoryItemShipmentTests
{
    [Fact]
    public void ShippingConvertsAReservationIntoAnActualDecrease()
    {
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(3, orderId, reservationId);
        item.DequeuePendingMovements();

        item.Ship(3, orderId, reservationId);

        Assert.Equal(7, item.QuantityOnHand);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(7, item.AvailableQuantity);
        StockMovement movement = Assert.Single(item.DequeuePendingMovements());
        Assert.Equal(StockMovementType.StockShipped, movement.Type);
        Assert.Equal(3, movement.Quantity);
        Assert.Equal(10, movement.QuantityOnHandBefore);
        Assert.Equal(7, movement.QuantityOnHandAfter);
        Assert.Equal(3, movement.ReservedQuantityBefore);
        Assert.Equal(0, movement.ReservedQuantityAfter);
        Assert.Equal(orderId, movement.OrderId);
    }

    [Fact]
    public void ShippingLeavesAvailableQuantityUnchanged()
    {
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(4, orderId, reservationId);
        int availableWhileReserved = item.AvailableQuantity;

        item.Ship(4, orderId, reservationId);

        Assert.Equal(availableWhileReserved, item.AvailableQuantity);
    }

    [Fact]
    public void ShippingNeverRemovesMoreThanTheReservedQuantity()
    {
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(2, orderId, reservationId);

        item.Ship(5, orderId, reservationId);

        Assert.Equal(8, item.QuantityOnHand);
        Assert.Equal(0, item.ReservedQuantity);
    }

    [Fact]
    public void ShippingWithoutAReservationChangesNothing()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.DequeuePendingMovements();

        item.Ship(3, Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(10, item.QuantityOnHand);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Empty(item.DequeuePendingMovements());
    }

    [Fact]
    public void ReleasingAfterShippingDoesNotResurrectStock()
    {
        Guid orderId = Guid.NewGuid();
        Guid reservationId = Guid.NewGuid();
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(3, orderId, reservationId);
        item.Ship(3, orderId, reservationId);

        item.Release(3, orderId, reservationId);

        Assert.Equal(7, item.QuantityOnHand);
        Assert.Equal(0, item.ReservedQuantity);
    }
}
