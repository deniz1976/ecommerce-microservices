namespace ECommerce.Shipping.Application.Shipments;

public sealed record CreateShipmentRequest(
    Guid OrderId,
    Guid CustomerId,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode);
