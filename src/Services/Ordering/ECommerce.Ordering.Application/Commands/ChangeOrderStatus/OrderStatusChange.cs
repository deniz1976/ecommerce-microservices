namespace ECommerce.Ordering.Application.Commands.ChangeOrderStatus;

public enum OrderStatusChange
{
    Confirm,
    Cancel,
    RejectCancellation,
    InventoryReserved,
    PaymentAuthorized,
    ShipmentCreated
}
