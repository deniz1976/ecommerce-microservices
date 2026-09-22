namespace ECommerce.Inventory.Domain;

public sealed class StockReservation
{
    private StockReservation()
    {
        FailureReason = string.Empty;
    }

    public StockReservation(Guid id, Guid orderId, Guid productId, int quantity, StockReservationStatus status, string? failureReason)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Status = status;
        FailureReason = failureReason ?? string.Empty;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public StockReservationStatus Status { get; private set; }

    public string FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void MarkReleased()
    {
        Status = StockReservationStatus.Released;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkShipped()
    {
        Status = StockReservationStatus.Shipped;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
