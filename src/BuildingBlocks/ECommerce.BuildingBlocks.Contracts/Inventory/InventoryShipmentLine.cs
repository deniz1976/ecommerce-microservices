namespace ECommerce.BuildingBlocks.Contracts.Inventory;

public sealed record InventoryShipmentLine(
    Guid ProductId,
    int Quantity);
