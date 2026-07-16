using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public static class BasketMapper
{
    public static BasketResponse ToResponse(this BasketEntity basket)
    {
        return new BasketResponse(
            basket.CustomerId,
            basket.Currency,
            basket.TotalAmount,
            basket.UpdatedAt,
            basket.Items
                .Select(x => new BasketItemResponse(x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.TotalPrice, x.Currency))
                .ToArray());
    }
}
