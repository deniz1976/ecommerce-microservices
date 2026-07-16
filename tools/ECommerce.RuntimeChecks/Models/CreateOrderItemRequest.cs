namespace ECommerce.RuntimeChecks.Models;

internal sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
