using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Queries.SearchManagedShipments;

public sealed record SearchManagedShipmentsQuery(ManagedShipmentListCriteria Criteria)
    : IQuery<PagedResult<ShipmentResponse>>;
