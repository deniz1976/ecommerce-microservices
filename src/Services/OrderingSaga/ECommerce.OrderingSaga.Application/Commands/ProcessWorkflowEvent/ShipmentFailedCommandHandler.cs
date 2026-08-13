using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class ShipmentFailedCommandHandler(ShippingWorkflowService service)
    : ICommandHandler<ProcessWorkflowEventCommand<ShipmentFailed>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<ShipmentFailed> command, CancellationToken cancellationToken) =>
        service.HandleFailedAsync(command.Event, cancellationToken);
}
