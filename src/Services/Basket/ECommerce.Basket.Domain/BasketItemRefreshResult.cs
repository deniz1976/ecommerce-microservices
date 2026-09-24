namespace ECommerce.Basket.Domain;

public enum BasketItemRefreshResult
{
    Unchanged = 1,
    DetailsChanged = 2,
    PriceChanged = 3,
    Rejected = 4
}
