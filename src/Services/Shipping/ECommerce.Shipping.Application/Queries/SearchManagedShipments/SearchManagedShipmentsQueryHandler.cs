using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Queries.SearchManagedShipments;

public sealed class SearchManagedShipmentsQueryHandler
    : IQueryHandler<SearchManagedShipmentsQuery, PagedResult<ShipmentResponse>>
{
    private readonly ShipmentQueryService queryService;

    public SearchManagedShipmentsQueryHandler(ShipmentQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<ShipmentResponse>> HandleAsync(
        SearchManagedShipmentsQuery query,
        CancellationToken cancellationToken) =>
        queryService.SearchAsync(query.Criteria, cancellationToken);
}
