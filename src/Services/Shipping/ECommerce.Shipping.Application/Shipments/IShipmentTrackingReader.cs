namespace ECommerce.Shipping.Application.Shipments;

public interface IShipmentTrackingReader
{
    Task<Guid?> FindIdByTrackingNumberAsync(
        string trackingNumber,
        CancellationToken cancellationToken);
}
