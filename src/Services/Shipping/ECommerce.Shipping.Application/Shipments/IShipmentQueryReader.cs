using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Shipping.Application.Shipments;

public interface IShipmentQueryReader
{
    Task<ShipmentResponse?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<PagedResult<ShipmentResponse>> SearchAsync(
        ManagedShipmentListCriteria criteria,
        CancellationToken cancellationToken);
}
