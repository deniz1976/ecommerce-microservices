using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class PaymentAuthorizedCommandHandler(
    PaymentWorkflowService service,
    OrderWorkflowTimeoutOptions options)
    : ICommandHandler<ProcessWorkflowEventCommand<PaymentAuthorized>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<PaymentAuthorized> command, CancellationToken cancellationToken) =>
        service.HandleAuthorizedAsync(command.Event, DateTimeOffset.UtcNow, options.ShippingTimeout, cancellationToken);
}
