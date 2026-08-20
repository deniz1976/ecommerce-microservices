using ECommerce.Shipping.Application.Shipments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShipmentTrackingReader(ShippingDbContext dbContext)
    : IShipmentTrackingReader
{
    public Task<Guid?> FindIdByTrackingNumberAsync(
        string trackingNumber,
        CancellationToken cancellationToken)
    {
        return dbContext.Shipments
            .AsNoTracking()
            .Where(shipment => shipment.TrackingNumber == trackingNumber)
            .Select(shipment => (Guid?)shipment.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
