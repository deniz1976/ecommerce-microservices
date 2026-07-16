namespace ECommerce.Ordering.Domain;

public sealed class OrderItem
{
    private OrderItem()
    {
        ProductName = string.Empty;
        Currency = string.Empty;
    }

    public OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Order? Order { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    public string Currency { get; private set; }
}
