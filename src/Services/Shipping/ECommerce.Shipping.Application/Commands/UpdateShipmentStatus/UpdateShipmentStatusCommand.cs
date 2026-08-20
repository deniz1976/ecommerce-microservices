using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Commands.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusCommand(ShipmentStatusUpdateRequest Request)
    : ICommand<ShipmentStatusUpdateResult>;
