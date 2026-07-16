namespace ECommerce.OrderingSaga.Domain;

public enum OrderWorkflowStatus
{
    Submitted = 1,
    InventoryReserved = 2,
    PaymentAuthorized = 3,
    ShipmentCreated = 4,
    Completed = 5,
    Cancelled = 6
}
