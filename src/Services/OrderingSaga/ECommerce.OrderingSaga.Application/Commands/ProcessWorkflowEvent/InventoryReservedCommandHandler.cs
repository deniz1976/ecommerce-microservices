using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class InventoryReservedCommandHandler(
    InventoryWorkflowService service,
    OrderWorkflowTimeoutOptions options)
    : ICommandHandler<ProcessWorkflowEventCommand<InventoryReserved>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<InventoryReserved> command, CancellationToken cancellationToken) =>
        service.HandleReservedAsync(command.Event, DateTimeOffset.UtcNow, options.PaymentTimeout, cancellationToken);
}
