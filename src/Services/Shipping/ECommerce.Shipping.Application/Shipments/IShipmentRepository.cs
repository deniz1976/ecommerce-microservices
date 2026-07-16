namespace ECommerce.Shipping.Application.Shipments;

public interface IShipmentRepository
{
    Task<Domain.Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Domain.Shipment shipment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
