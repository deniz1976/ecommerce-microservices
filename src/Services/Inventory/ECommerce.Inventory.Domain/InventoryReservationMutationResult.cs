namespace ECommerce.Inventory.Domain;

public enum InventoryReservationMutationResult
{
    Applied = 0,
    InvalidQuantity = 1,
    InsufficientStock = 2
}
