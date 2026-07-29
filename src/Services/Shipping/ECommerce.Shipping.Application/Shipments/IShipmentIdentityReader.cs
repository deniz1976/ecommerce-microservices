namespace ECommerce.Shipping.Application.Shipments;

public interface IShipmentIdentityReader
{
    Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
