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
    public void Reserve_increases_reserved_quantity_and_reduces_available_quantity()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);

        item.Reserve(3);

        Assert.Equal(3, item.ReservedQuantity);
        Assert.Equal(7, item.AvailableQuantity);
    }

    [Fact]
    public void Release_never_makes_reserved_quantity_negative()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(2);

        item.Release(5);

        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(10, item.AvailableQuantity);
    }

    [Fact]
    public void SetQuantityOnHand_updates_total_stock_without_changing_reserved_quantity()
    {
        InventoryItem item = new(Guid.NewGuid(), 10);
        item.Reserve(4);

        item.SetQuantityOnHand(12);

        Assert.Equal(12, item.QuantityOnHand);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.Equal(8, item.AvailableQuantity);
    }
}
