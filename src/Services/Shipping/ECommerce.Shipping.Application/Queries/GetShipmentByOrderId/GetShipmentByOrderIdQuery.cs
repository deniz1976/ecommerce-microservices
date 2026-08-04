using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Queries.GetShipmentByOrderId;

public sealed record GetShipmentByOrderIdQuery(Guid OrderId)
    : IQuery<Result<ShipmentResponse>>;
