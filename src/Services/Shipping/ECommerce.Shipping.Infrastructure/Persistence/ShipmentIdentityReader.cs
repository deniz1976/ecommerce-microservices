using ECommerce.Shipping.Application.Shipments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShipmentIdentityReader : IShipmentIdentityReader
{
    private readonly ShippingDbContext dbContext;

    public ShipmentIdentityReader(ShippingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.Shipments
            .AsNoTracking()
            .Where(shipment => shipment.OrderId == orderId)
            .Select(shipment => (Guid?)shipment.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
