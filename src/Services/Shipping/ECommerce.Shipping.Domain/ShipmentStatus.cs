namespace ECommerce.Shipping.Domain;

public enum ShipmentStatus
{
    Created = 1,
    Failed = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5
}
