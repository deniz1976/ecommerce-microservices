namespace ECommerce.Ordering.Domain;

public sealed class OrderStatusHistory
{
    private OrderStatusHistory()
    {
    }

    public OrderStatusHistory(
        Guid orderId,
        OrderStatus status,
        DateTimeOffset occurredAt,
        string? reasonCode)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Status = status;
        OccurredAt = occurredAt;
        ReasonCode = reasonCode;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string? ReasonCode { get; private set; }

    public Order? Order { get; private set; }
}
