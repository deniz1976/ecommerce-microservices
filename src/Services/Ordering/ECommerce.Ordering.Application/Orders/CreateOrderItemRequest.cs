namespace ECommerce.Ordering.Application.Orders;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
