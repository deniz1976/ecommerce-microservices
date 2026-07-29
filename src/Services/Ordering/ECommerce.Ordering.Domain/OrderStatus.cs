namespace ECommerce.Ordering.Domain;

public enum OrderStatus
{
    Submitted = 0,
    InventoryReserved = 1,
    PaymentAuthorized = 2,
    ShipmentCreated = 3,
    Confirmed = 4,
    Cancelled = 5
}
