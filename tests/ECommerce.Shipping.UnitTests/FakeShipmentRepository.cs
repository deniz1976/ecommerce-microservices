using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

internal sealed class FakeShipmentRepository : IShipmentRepository
{
    public Shipment? Shipment { get; private set; }

    public int SaveChangesCount { get; private set; }

    public Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult(Shipment?.OrderId == orderId ? Shipment : null);

    public void Add(Shipment shipment) => Shipment = shipment;

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
