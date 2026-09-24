using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Commands.CancelShipment;

public sealed class CancelShipmentCommandHandler(ShipmentCancellationService service)
    : ICommandHandler<CancelShipmentCommand, ShipmentCancellationResult>
{
    public Task<ShipmentCancellationResult> HandleAsync(
        CancelShipmentCommand command,
        CancellationToken cancellationToken)
    {
        return service.CancelAsync(command.Request, cancellationToken);
    }
}
