namespace ECommerce.Shipping.Domain;

public sealed class InvalidShipmentStatusUpdateException : Exception
{
    public InvalidShipmentStatusUpdateException(string message)
        : base(message)
    {
    }
}
