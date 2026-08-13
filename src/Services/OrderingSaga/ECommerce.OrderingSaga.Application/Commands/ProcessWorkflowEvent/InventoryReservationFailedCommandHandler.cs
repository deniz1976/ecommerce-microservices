using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class InventoryReservationFailedCommandHandler(InventoryWorkflowService service)
    : ICommandHandler<ProcessWorkflowEventCommand<InventoryReservationFailed>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<InventoryReservationFailed> command, CancellationToken cancellationToken) =>
        service.HandleFailedAsync(command.Event, cancellationToken);
}
