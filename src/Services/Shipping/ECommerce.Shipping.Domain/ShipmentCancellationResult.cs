namespace ECommerce.Shipping.Domain;

public enum ShipmentCancellationResult
{
    Cancelled = 1,
    AlreadyCancelled = 2,
    NotCancellable = 3,
    NotFound = 4
}
