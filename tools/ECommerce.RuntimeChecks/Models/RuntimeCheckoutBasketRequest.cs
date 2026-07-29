namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeCheckoutBasketRequest(
    Guid CheckoutId,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode);
