using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;

namespace ECommerce.OrderingSaga.Application.Commands.ProcessWorkflowEvent;

public sealed class PaymentFailedCommandHandler(PaymentWorkflowService service)
    : ICommandHandler<ProcessWorkflowEventCommand<PaymentFailed>>
{
    public Task HandleAsync(ProcessWorkflowEventCommand<PaymentFailed> command, CancellationToken cancellationToken) =>
        service.HandleFailedAsync(command.Event, cancellationToken);
}
