using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Shipping.Application.Shipments;

public sealed class ShipmentQueryService
{
    private readonly IShipmentQueryReader shipmentReader;

    public ShipmentQueryService(IShipmentQueryReader shipmentReader)
    {
        this.shipmentReader = shipmentReader;
    }

    public async Task<Result<ShipmentResponse>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        ShipmentResponse? shipment = await shipmentReader.GetByOrderIdAsync(
            orderId,
            cancellationToken);
        return shipment is null
            ? Result<ShipmentResponse>.Failure(
                new Error(ErrorCodes.ShipmentNotFound, ErrorCodes.ShipmentNotFound))
            : Result<ShipmentResponse>.Success(shipment);
    }

    public Task<PagedResult<ShipmentResponse>> SearchAsync(
        ManagedShipmentListCriteria criteria,
        CancellationToken cancellationToken) =>
        shipmentReader.SearchAsync(criteria, cancellationToken);
}
