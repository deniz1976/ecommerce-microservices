namespace ECommerce.Ordering.Application.Orders;

public sealed record SellerOrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency);
