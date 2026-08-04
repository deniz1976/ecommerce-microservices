using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Queries.GetShipmentByOrderId;

public sealed class GetShipmentByOrderIdQueryHandler
    : IQueryHandler<GetShipmentByOrderIdQuery, Result<ShipmentResponse>>
{
    private readonly ShipmentQueryService queryService;

    public GetShipmentByOrderIdQueryHandler(ShipmentQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<ShipmentResponse>> HandleAsync(
        GetShipmentByOrderIdQuery query,
        CancellationToken cancellationToken) =>
        queryService.GetByOrderIdAsync(query.OrderId, cancellationToken);
}
