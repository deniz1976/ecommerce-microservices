namespace ECommerce.Basket.Domain;

public sealed class Basket
{
    private readonly List<BasketItem> items = [];

    private Basket()
    {
        Currency = string.Empty;
    }

    public Basket(Guid customerId, string currency)
    {
        CustomerId = customerId;
        Currency = currency;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Basket(
        Guid customerId,
        string currency,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<BasketItem> restoredItems)
    {
        CustomerId = customerId;
        Currency = currency;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = updatedAt.ToUniversalTime();
        items.AddRange(restoredItems);
    }

    public Guid CustomerId { get; private set; }

    public string Currency { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<BasketItem> Items => items;

    public decimal TotalAmount => items.Sum(x => x.TotalPrice);

    public void AddOrUpdateItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        BasketItem? item = items.FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
        {
            items.Add(new BasketItem(productId, productName, quantity, unitPrice, currency));
        }
        else
        {
            item.Update(productName, quantity, unitPrice, currency);
        }

        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        BasketItem? item = items.FirstOrDefault(x => x.ProductId == productId);

        if (item is not null)
        {
            items.Remove(item);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Clear()
    {
        items.Clear();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
