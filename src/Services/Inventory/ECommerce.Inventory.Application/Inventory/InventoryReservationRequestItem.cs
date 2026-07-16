namespace ECommerce.Inventory.Application.Inventory;

public sealed record InventoryReservationRequestItem(
    Guid ProductId,
    int Quantity);
