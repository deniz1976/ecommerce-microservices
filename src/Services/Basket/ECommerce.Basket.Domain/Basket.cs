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

    public BasketItemMutationResult AddOrUpdateItem(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string currency,
        Guid? storeId = null)
    {
        if (productId == Guid.Empty ||
            string.IsNullOrWhiteSpace(productName) ||
            quantity <= 0 ||
            unitPrice < 0 ||
            string.IsNullOrWhiteSpace(currency))
        {
            return BasketItemMutationResult.InvalidItem;
        }

        if (items.Count > 0 &&
            !string.Equals(Currency, currency, StringComparison.OrdinalIgnoreCase))
        {
            return BasketItemMutationResult.CurrencyMismatch;
        }

        BasketItem? item = items.FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
        {
            items.Add(new BasketItem(
                productId,
                productName,
                quantity,
                unitPrice,
                currency,
                storeId));
        }
        else
        {
            item.Update(productName, quantity, unitPrice, currency, storeId);
        }

        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
        return BasketItemMutationResult.Applied;
    }

    public BasketItemRefreshResult RefreshItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        Guid? storeId)
    {
        BasketItem? item = items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null ||
            string.IsNullOrWhiteSpace(productName) ||
            unitPrice < 0 ||
            !string.Equals(Currency, currency, StringComparison.OrdinalIgnoreCase))
        {
            return BasketItemRefreshResult.Rejected;
        }

        bool priceChanged = item.UnitPrice != unitPrice;
        bool detailsChanged = item.ProductName != productName || item.StoreId != storeId;
        if (!priceChanged && !detailsChanged)
        {
            return BasketItemRefreshResult.Unchanged;
        }

        item.Update(productName, item.Quantity, unitPrice, item.Currency, storeId);
        UpdatedAt = DateTimeOffset.UtcNow;
        return priceChanged
            ? BasketItemRefreshResult.PriceChanged
            : BasketItemRefreshResult.DetailsChanged;
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
