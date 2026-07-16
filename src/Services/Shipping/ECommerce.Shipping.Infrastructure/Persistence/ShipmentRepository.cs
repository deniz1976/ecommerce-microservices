using ECommerce.Shipping.Application.Shipments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly ShippingDbContext dbContext;

    public ShipmentRepository(ShippingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Domain.Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.Shipments.SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public void Add(Domain.Shipment shipment)
    {
        dbContext.Shipments.Add(shipment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
