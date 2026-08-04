namespace ECommerce.Ordering.Domain;

public sealed class Order
{
    private readonly List<OrderItem> items = [];
    private readonly List<OrderStatusHistory> statusHistory = [];

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
        statusHistory.Add(new OrderStatusHistory(Id, Status, CreatedAt, null));
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

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => statusHistory;

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        items.Add(new OrderItem(Id, productId, productName, quantity, unitPrice, currency));
        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkConfirmed()
    {
        AdvanceTo(OrderStatus.Confirmed);
    }

    public void MarkCancellationRequested()
    {
        if (Status is OrderStatus.Confirmed or
            OrderStatus.Cancelled or
            OrderStatus.CancellationRequested or
            OrderStatus.ShipmentCreated)
        {
            return;
        }

        Status = OrderStatus.CancellationRequested;
        UpdatedAt = DateTimeOffset.UtcNow;
        statusHistory.Add(new OrderStatusHistory(Id, Status, UpdatedAt, null));
    }

    public void MarkCancellationRejected()
    {
        if (Status != OrderStatus.CancellationRequested)
        {
            return;
        }

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
        statusHistory.Add(new OrderStatusHistory(Id, Status, UpdatedAt, null));
    }

    public void MarkCancelled(string? reasonCode = null)
    {
        if (Status is OrderStatus.Confirmed or OrderStatus.Cancelled)
        {
            return;
        }

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        statusHistory.Add(new OrderStatusHistory(Id, Status, UpdatedAt, reasonCode));
    }

    public void MarkInventoryReserved()
    {
        AdvanceTo(OrderStatus.InventoryReserved);
    }

    public void MarkPaymentAuthorized()
    {
        AdvanceTo(OrderStatus.PaymentAuthorized);
    }

    public void MarkShipmentCreated()
    {
        AdvanceTo(OrderStatus.ShipmentCreated);
    }

    private void AdvanceTo(OrderStatus next)
    {
        if (Status is OrderStatus.Confirmed or OrderStatus.Cancelled || next <= Status)
        {
            return;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        for (int value = (int)Status + 1; value <= (int)next; value++)
        {
            statusHistory.Add(new OrderStatusHistory(Id, (OrderStatus)value, UpdatedAt, null));
        }

        Status = next;
    }
}
