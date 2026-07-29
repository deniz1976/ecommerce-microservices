using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

internal sealed class FakeShipmentRepository :
    IRepository<Shipment, Guid>,
    IUnitOfWork,
    IShipmentIdentityReader
{
    public Shipment? Shipment { get; private set; }

    public int SaveChangesCount { get; private set; }

    public Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult<Guid?>(Shipment?.OrderId == orderId ? Shipment.Id : null);

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Shipment?.Id == id ? Shipment : null);

    public void Add(Shipment shipment) => Shipment = shipment;

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
