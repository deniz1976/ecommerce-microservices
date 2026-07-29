using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class ProcessWorkflowEventCommandHandler<TEvent>
    : ICommandHandler<ProcessWorkflowEventCommand<TEvent>>
    where TEvent : class
{
    private readonly OrderWorkflowService workflowService;

    public ProcessWorkflowEventCommandHandler(OrderWorkflowService workflowService)
    {
        this.workflowService = workflowService;
    }

    public Task HandleAsync(
        ProcessWorkflowEventCommand<TEvent> command,
        CancellationToken cancellationToken)
    {
        return command.Event switch
        {
            OrderSubmitted message => workflowService.HandleAsync(message, cancellationToken),
            InventoryReserved message => workflowService.HandleAsync(message, cancellationToken),
            InventoryReservationFailed message => workflowService.HandleAsync(
                message,
                cancellationToken),
            PaymentAuthorized message => workflowService.HandleAsync(message, cancellationToken),
            PaymentFailed message => workflowService.HandleAsync(message, cancellationToken),
            ShipmentCreated message => workflowService.HandleAsync(message, cancellationToken),
            ShipmentFailed message => workflowService.HandleAsync(message, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(command),
                typeof(TEvent).FullName,
                "Unsupported ordering workflow event.")
        };
    }
}
