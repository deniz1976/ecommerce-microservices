using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Commands.CancelShipment;

public sealed record CancelShipmentCommand(ShipmentCancellationRequest Request)
    : ICommand<ShipmentCancellationResult>;
