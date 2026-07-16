namespace ECommerce.Basket.Application.Baskets;

public sealed record AddBasketItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
