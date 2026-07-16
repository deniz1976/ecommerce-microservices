namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryReservationResult(
    bool Succeeded,
    string? ReasonCode,
    string? Reason);
