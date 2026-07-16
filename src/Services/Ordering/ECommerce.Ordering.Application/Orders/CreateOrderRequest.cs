namespace ECommerce.Ordering.Application.Orders;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string Currency,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode,
    IReadOnlyCollection<CreateOrderItemRequest> Items);
