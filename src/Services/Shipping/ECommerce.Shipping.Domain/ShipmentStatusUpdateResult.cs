namespace ECommerce.Shipping.Domain;

public enum ShipmentStatusUpdateResult
{
    Applied = 1,
    Duplicate = 2,
    Stale = 3,
    Terminal = 4
}
