namespace ECommerce.Basket.Application.Baskets;

public sealed record BasketResponse(
    Guid CustomerId,
    string Currency,
    decimal TotalAmount,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<BasketItemResponse> Items);
