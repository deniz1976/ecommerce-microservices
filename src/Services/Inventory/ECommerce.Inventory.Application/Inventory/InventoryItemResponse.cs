namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryItemResponse(
    Guid ProductId,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTimeOffset UpdatedAt);
