using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record OrderStatusHistoryResponse(
    OrderStatus Status,
    DateTimeOffset OccurredAt,
    string? ReasonCode);
