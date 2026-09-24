using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed class ShipmentCancellationService(
    IRepository<Domain.Shipment, Guid> shipmentRepository,
    IShipmentIdentityReader identityReader,
    IUnitOfWork unitOfWork)
{
    public async Task<ShipmentCancellationResult> CancelAsync(
        ShipmentCancellationRequest request,
        CancellationToken cancellationToken)
    {
        Guid? shipmentId = await identityReader.FindIdByOrderIdAsync(
            request.OrderId,
            cancellationToken);
        Domain.Shipment? shipment = shipmentId is null
            ? null
            : await shipmentRepository.GetByIdAsync(shipmentId.Value, cancellationToken);
        if (shipment is null)
        {
            return ShipmentCancellationResult.NotFound;
        }

        ShipmentCancellationResult result = shipment.Cancel(request.Reason);
        if (result == ShipmentCancellationResult.Cancelled)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
