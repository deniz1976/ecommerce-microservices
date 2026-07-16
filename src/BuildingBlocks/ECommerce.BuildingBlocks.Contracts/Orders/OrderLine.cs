namespace ECommerce.BuildingBlocks.Contracts.Orders;

public sealed record OrderLine(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
