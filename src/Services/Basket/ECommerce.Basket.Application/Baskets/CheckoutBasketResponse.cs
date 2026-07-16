namespace ECommerce.Basket.Application.Baskets;

public sealed record CheckoutBasketResponse(
    Guid SnapshotId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt);
