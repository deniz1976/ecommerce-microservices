using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class OrderSubmittedCommandHandler(
    OrderSubmissionWorkflowService service,
    OrderWorkflowTimeoutOptions options)
    : ICommandHandler<ProcessWorkflowEventCommand<OrderSubmitted>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<OrderSubmitted> command, CancellationToken cancellationToken) =>
        service.HandleAsync(command.Event, DateTimeOffset.UtcNow, options.InventoryTimeout, cancellationToken);
}
