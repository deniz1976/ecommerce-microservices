using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Commands.CreateShipment;

public sealed record CreateShipmentCommand(CreateShipmentRequest Request)
    : ICommand<CreateShipmentResult>;
