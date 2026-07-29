using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    OrderStatus Status,
    decimal TotalAmount,
    string RecipientName,
    string AddressLine,
    string City,
    string CountryCode,
    string PostalCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<OrderItemResponse> Items,
    IReadOnlyCollection<OrderStatusHistoryResponse> StatusHistory);
