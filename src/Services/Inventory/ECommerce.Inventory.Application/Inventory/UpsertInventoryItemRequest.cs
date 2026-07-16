namespace ECommerce.Inventory.Application.Inventory;

public sealed record UpsertInventoryItemRequest(
    Guid ProductId,
    int QuantityOnHand);
