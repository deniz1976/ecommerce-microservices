namespace ECommerce.Basket.Application.Baskets;

public sealed record AddBasketItemRequest(
    Guid ProductId,
    int Quantity);
