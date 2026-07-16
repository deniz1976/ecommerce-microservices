using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<OrderItemResponse> Items);
