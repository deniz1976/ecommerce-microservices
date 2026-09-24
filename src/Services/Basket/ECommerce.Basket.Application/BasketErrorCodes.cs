namespace ECommerce.Basket.Application;

public static class BasketErrorCodes
{
    public const string EmptyBasket = "EMPTY_BASKET";
    public const string InvalidBasketItem = "INVALID_BASKET_ITEM";
    public const string ProductCatalogUnavailable = "PRODUCT_CATALOG_UNAVAILABLE";
    public const string BasketStoreUnavailable = "BASKET_STORE_UNAVAILABLE";
    public const string CurrencyMismatch = "BASKET_CURRENCY_MISMATCH";
    public const string InvalidCheckoutAddress = "INVALID_CHECKOUT_ADDRESS";
    public const string BasketTooLarge = "BASKET_TOO_LARGE";
    public const string CheckoutConflict = "BASKET_CHECKOUT_CONFLICT";
    public const string BasketPricesChanged = "BASKET_PRICES_CHANGED";
    public const string BasketItemUnavailable = "BASKET_ITEM_UNAVAILABLE";
    public const string BasketConcurrentUpdate = "BASKET_CONCURRENT_UPDATE";
}
