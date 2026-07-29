namespace ECommerce.Basket.Domain;

public sealed class BasketCheckoutSnapshot
{
    private readonly List<BasketCheckoutSnapshotItem> items = [];

    private BasketCheckoutSnapshot()
    {
        Currency = string.Empty;
        RecipientName = string.Empty;
        AddressLine = string.Empty;
        City = string.Empty;
        CountryCode = string.Empty;
        PostalCode = string.Empty;
    }

    public BasketCheckoutSnapshot(
        Guid id,
        Guid customerId,
        string currency,
        decimal totalAmount,
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode)
    {
        Id = id;
        CustomerId = customerId;
        Currency = currency;
        TotalAmount = totalAmount;
        RecipientName = recipientName;
        AddressLine = addressLine;
        City = city;
        CountryCode = countryCode;
        PostalCode = postalCode;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Currency { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string RecipientName { get; private set; }

    public string AddressLine { get; private set; }

    public string City { get; private set; }

    public string CountryCode { get; private set; }

    public string PostalCode { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<BasketCheckoutSnapshotItem> Items => items;

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        items.Add(new BasketCheckoutSnapshotItem(Id, productId, productName, quantity, unitPrice, currency));
    }
}
