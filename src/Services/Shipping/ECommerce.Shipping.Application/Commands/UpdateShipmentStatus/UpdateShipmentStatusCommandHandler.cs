using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Commands.UpdateShipmentStatus;

public sealed class UpdateShipmentStatusCommandHandler(ShipmentStatusUpdateService service)
    : ICommandHandler<UpdateShipmentStatusCommand, ShipmentStatusUpdateResult>
{
    public Task<ShipmentStatusUpdateResult> HandleAsync(
        UpdateShipmentStatusCommand command,
        CancellationToken cancellationToken)
    {
        return service.UpdateAsync(command.Request, cancellationToken);
    }
}
