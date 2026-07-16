namespace ECommerce.OrderingSaga.Domain;

public sealed class OrderWorkflow
{
    private readonly List<OrderWorkflowItem> items = [];

    private OrderWorkflow()
    {
        Currency = string.Empty;
        RecipientName = string.Empty;
        AddressLine = string.Empty;
        City = string.Empty;
        CountryCode = string.Empty;
        PostalCode = string.Empty;
    }

    public OrderWorkflow(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency,
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
        RecipientName = recipientName;
        AddressLine = addressLine;
        City = city;
        CountryCode = countryCode;
        PostalCode = postalCode;
        Status = OrderWorkflowStatus.Submitted;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; }

    public string RecipientName { get; private set; }

    public string AddressLine { get; private set; }

    public string City { get; private set; }

    public string CountryCode { get; private set; }

    public string PostalCode { get; private set; }

    public OrderWorkflowStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderWorkflowItem> Items => items;

    public void AddItem(Guid productId, int quantity)
    {
        items.Add(new OrderWorkflowItem(Id, productId, quantity));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkInventoryReserved()
    {
        if (Status == OrderWorkflowStatus.Submitted)
        {
            Status = OrderWorkflowStatus.InventoryReserved;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkPaymentAuthorized()
    {
        if (Status == OrderWorkflowStatus.InventoryReserved)
        {
            Status = OrderWorkflowStatus.PaymentAuthorized;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkShipmentCreated()
    {
        if (Status == OrderWorkflowStatus.PaymentAuthorized)
        {
            Status = OrderWorkflowStatus.ShipmentCreated;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCompleted()
    {
        if (Status == OrderWorkflowStatus.ShipmentCreated)
        {
            Status = OrderWorkflowStatus.Completed;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCancelled(string reason)
    {
        if (Status is OrderWorkflowStatus.Completed or OrderWorkflowStatus.Cancelled)
        {
            return;
        }

        Status = OrderWorkflowStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
