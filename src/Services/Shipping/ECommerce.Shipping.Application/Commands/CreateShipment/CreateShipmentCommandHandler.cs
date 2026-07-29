using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;

namespace ECommerce.Shipping.Application.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler
    : ICommandHandler<CreateShipmentCommand, CreateShipmentResult>
{
    private readonly ShipmentService shipmentService;

    public CreateShipmentCommandHandler(ShipmentService shipmentService)
    {
        this.shipmentService = shipmentService;
    }

    public Task<CreateShipmentResult> HandleAsync(
        CreateShipmentCommand command,
        CancellationToken cancellationToken)
    {
        return shipmentService.CreateAsync(command.Request, cancellationToken);
    }
}
