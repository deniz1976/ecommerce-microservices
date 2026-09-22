namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryShipmentRequest(
    Guid OrderId,
    Guid CustomerId);
