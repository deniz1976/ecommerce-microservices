namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryReleaseRequest(
    Guid OrderId,
    Guid CustomerId);
