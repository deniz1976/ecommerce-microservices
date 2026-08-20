using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed class ShipmentStatusUpdateService(
    IRepository<Domain.Shipment, Guid> shipmentRepository,
    IShipmentTrackingReader trackingReader,
    IUnitOfWork unitOfWork)
{
    public async Task<ShipmentStatusUpdateResult> UpdateAsync(
        ShipmentStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        string trackingNumber = request.TrackingNumber.Trim();
        if (trackingNumber.Length == 0)
        {
            throw new InvalidShipmentStatusUpdateException("Tracking number is required.");
        }

        Guid? shipmentId = await trackingReader.FindIdByTrackingNumberAsync(
            trackingNumber,
            cancellationToken);
        if (!shipmentId.HasValue)
        {
            return ShipmentStatusUpdateResult.Stale;
        }

        Domain.Shipment? shipment = await shipmentRepository.GetByIdAsync(
            shipmentId.Value,
            cancellationToken);
        if (shipment is null)
        {
            return ShipmentStatusUpdateResult.Stale;
        }

        ShipmentStatusUpdateResult result = shipment.ApplyStatusUpdate(
            request.UpdateId,
            request.Status,
            request.OccurredAt);
        if (result == ShipmentStatusUpdateResult.Applied)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
