namespace ECommerce.Ordering.Domain;

public sealed class Order
{
    private readonly List<OrderItem> items = [];

    private Order()
    {
        Currency = string.Empty;
        RecipientName = string.Empty;
        AddressLine = string.Empty;
        City = string.Empty;
        CountryCode = string.Empty;
        PostalCode = string.Empty;
    }

    public Order(Guid id, Guid customerId, string currency, string recipientName, string addressLine, string city, string countryCode, string postalCode)
    {
        Id = id;
        CustomerId = customerId;
        Currency = currency;
        RecipientName = recipientName;
        AddressLine = addressLine;
        City = city;
        CountryCode = countryCode;
        PostalCode = postalCode;
        Status = OrderStatus.Submitted;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Currency { get; private set; }

    public string RecipientName { get; private set; }

    public string AddressLine { get; private set; }

    public string City { get; private set; }

    public string CountryCode { get; private set; }

    public string PostalCode { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount => items.Sum(x => x.TotalPrice);

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => items;

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        items.Add(new OrderItem(Id, productId, productName, quantity, unitPrice, currency));
        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkConfirmed()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
