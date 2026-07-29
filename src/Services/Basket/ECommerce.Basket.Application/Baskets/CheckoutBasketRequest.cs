namespace ECommerce.Basket.Application.Baskets;

public sealed record CheckoutBasketRequest(
    Guid CheckoutId,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode);
