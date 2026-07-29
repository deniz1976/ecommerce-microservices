namespace ECommerce.Ordering.Application.Commands.ChangeOrderStatus;

public enum OrderStatusChange
{
    Confirm,
    Cancel,
    InventoryReserved,
    PaymentAuthorized,
    ShipmentCreated
}
