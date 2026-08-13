using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class ProcessWorkflowEventCommandHandler<TEvent>
    : ICommandHandler<ProcessWorkflowEventCommand<TEvent>>
    where TEvent : class
{
    private readonly OrderWorkflowService workflowService;
    private readonly OrderWorkflowTimeoutOptions timeoutOptions;

    public ProcessWorkflowEventCommandHandler(
        OrderWorkflowService workflowService,
        OrderWorkflowTimeoutOptions timeoutOptions)
    {
        this.workflowService = workflowService;
        this.timeoutOptions = timeoutOptions;
    }

    public Task HandleAsync(
        ProcessWorkflowEventCommand<TEvent> command,
        CancellationToken cancellationToken)
    {
        return command.Event switch
        {
            OrderSubmitted message => workflowService.HandleAsync(
                message,
                DateTimeOffset.UtcNow,
                timeoutOptions.InventoryTimeout,
                cancellationToken),
            InventoryReserved message => workflowService.HandleAsync(
                message,
                DateTimeOffset.UtcNow,
                timeoutOptions.PaymentTimeout,
                cancellationToken),
            InventoryReservationFailed message => workflowService.HandleAsync(
                message,
                cancellationToken),
            PaymentAuthorized message => workflowService.HandleAsync(
                message,
                DateTimeOffset.UtcNow,
                timeoutOptions.ShippingTimeout,
                cancellationToken),
            PaymentFailed message => workflowService.HandleAsync(message, cancellationToken),
            ShipmentCreated message => workflowService.HandleAsync(message, cancellationToken),
            ShipmentFailed message => workflowService.HandleAsync(message, cancellationToken),
            OrderCancellationRequested message => workflowService.HandleAsync(
                message,
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(command),
                typeof(TEvent).FullName,
                "Unsupported ordering workflow event.")
        };
    }
}
