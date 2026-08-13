using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class ShipmentCreatedCommandHandler(ShippingWorkflowService service)
    : ICommandHandler<ProcessWorkflowEventCommand<ShipmentCreated>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<ShipmentCreated> command, CancellationToken cancellationToken) =>
        service.HandleCreatedAsync(command.Event, cancellationToken);
}
