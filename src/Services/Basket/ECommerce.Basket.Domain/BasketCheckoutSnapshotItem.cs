namespace ECommerce.Basket.Domain;

public sealed class BasketCheckoutSnapshotItem
{
    private BasketCheckoutSnapshotItem()
    {
        ProductName = string.Empty;
        Currency = string.Empty;
    }

    public BasketCheckoutSnapshotItem(
        Guid basketCheckoutSnapshotId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string currency,
        Guid? storeId = null)
    {
        Id = Guid.NewGuid();
        BasketCheckoutSnapshotId = basketCheckoutSnapshotId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
        StoreId = storeId;
    }

    public Guid Id { get; private set; }

    public Guid BasketCheckoutSnapshotId { get; private set; }

    public BasketCheckoutSnapshot? BasketCheckoutSnapshot { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    public string Currency { get; private set; }

    public Guid? StoreId { get; private set; }
}
