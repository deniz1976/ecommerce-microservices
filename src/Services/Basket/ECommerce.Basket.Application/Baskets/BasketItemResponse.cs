namespace ECommerce.Basket.Application.Baskets;

public sealed record BasketItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency);
