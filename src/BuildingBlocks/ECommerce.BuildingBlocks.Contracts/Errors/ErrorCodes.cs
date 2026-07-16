namespace ECommerce.BuildingBlocks.Contracts.Errors;

public static class ErrorCodes
{
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string BasketNotFound = "BASKET_NOT_FOUND";
    public const string OrderNotFound = "ORDER_NOT_FOUND";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string PaymentFailed = "PAYMENT_FAILED";
    public const string ShipmentFailed = "SHIPMENT_FAILED";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
}
