namespace ECommerce.Inventory.Domain;

public sealed class InventoryItem
{
    private InventoryItem()
    {
    }

    public InventoryItem(Guid productId, int quantityOnHand)
    {
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        ReservedQuantity = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ProductId { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool CanReserve(int quantity)
    {
        return quantity > 0 && AvailableQuantity >= quantity;
    }

    public void Reserve(int quantity)
    {
        ReservedQuantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Release(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        QuantityOnHand += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetQuantityOnHand(int quantityOnHand)
    {
        QuantityOnHand = quantityOnHand;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
