namespace ECommerce.BuildingBlocks.Contracts.Inventory;

public sealed record InventoryReleaseLine(
    Guid ProductId,
    int Quantity);
