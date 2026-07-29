namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeBasketItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency);
