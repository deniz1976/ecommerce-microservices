using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class OrderCancellationRequestedCommandHandler(OrderWorkflowCancellationService service)
    : ICommandHandler<ProcessWorkflowEventCommand<OrderCancellationRequested>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<OrderCancellationRequested> command, CancellationToken cancellationToken) =>
        service.HandleAsync(command.Event, cancellationToken);
}
