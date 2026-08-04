using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed record SellerOrderSummaryResponse(
    Guid OrderId,
    Guid StoreId,
    OrderStatus Status,
    string Currency,
    decimal StoreTotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<SellerOrderItemResponse> Items);
