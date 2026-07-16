namespace ECommerce.BuildingBlocks.Contracts.Inventory;

public sealed record InventoryReservationLine(
    Guid ProductId,
    int Quantity);
