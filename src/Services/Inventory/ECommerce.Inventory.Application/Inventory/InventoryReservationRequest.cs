namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryReservationRequest(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyCollection<InventoryReservationRequestItem> Items);
