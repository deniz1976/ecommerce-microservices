namespace ECommerce.RuntimeChecks.Configuration;

public enum WorkflowScenario
{
    All,
    BasketCheckout,
    Success,
    InventoryFailure,
    PaymentFailure,
    PaymentDecline,
    NotificationSignalR,
    SellerAuthorization,
    ShippingFailure
}
