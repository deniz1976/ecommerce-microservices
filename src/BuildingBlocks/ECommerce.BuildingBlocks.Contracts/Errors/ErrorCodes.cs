namespace ECommerce.BuildingBlocks.Contracts.Errors;

public static class ErrorCodes
{
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string BasketNotFound = "BASKET_NOT_FOUND";
    public const string OrderNotFound = "ORDER_NOT_FOUND";
    public const string OrderNotCancellable = "ORDER_NOT_CANCELLABLE";
    public const string OrderCancelledByCustomer = "ORDER_CANCELLED_BY_CUSTOMER";
    public const string InventoryTimeout = "INVENTORY_TIMEOUT";
    public const string PaymentTimeout = "PAYMENT_TIMEOUT";
    public const string ShipmentTimeout = "SHIPMENT_TIMEOUT";
    public const string PaymentNotFound = "PAYMENT_NOT_FOUND";
    public const string ShipmentNotFound = "SHIPMENT_NOT_FOUND";
    public const string NotificationNotFound = "NOTIFICATION_NOT_FOUND";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string StockBelowReserved = "STOCK_BELOW_RESERVED";
    public const string PaymentFailed = "PAYMENT_FAILED";
    public const string ShipmentFailed = "SHIPMENT_FAILED";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string AuthenticationRequired = "AUTHENTICATION_REQUIRED";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string DependencyUnavailable = "DEPENDENCY_UNAVAILABLE";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
}
