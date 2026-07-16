namespace ECommerce.Basket.Domain;

public sealed class BasketCheckoutSnapshot
{
    private readonly List<BasketCheckoutSnapshotItem> items = [];

    private BasketCheckoutSnapshot()
    {
        Currency = string.Empty;
    }

    public BasketCheckoutSnapshot(Guid id, Guid customerId, string currency, decimal totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        Currency = currency;
        TotalAmount = totalAmount;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Currency { get; private set; }

    public decimal TotalAmount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<BasketCheckoutSnapshotItem> Items => items;

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        items.Add(new BasketCheckoutSnapshotItem(Id, productId, productName, quantity, unitPrice, currency));
    }
}
