using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowCancellationService(
    IUnitOfWork unitOfWork,
    OrderWorkflowLoader loader,
    IWorkflowCommandPublisher publisher)
{
    public async Task HandleAsync(
        OrderCancellationRequested message,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(
            message.OrderId,
            cancellationToken);
        if (workflow is null)
        {
            throw new OrderWorkflowNotReadyException(message.OrderId);
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            return;
        }

        if (workflow.Status >= OrderWorkflowStatus.PaymentAuthorized)
        {
            await publisher.RejectOrderCancellationAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                cancellationToken);
            return;
        }

        OrderWorkflowStatus statusBeforeCancellation = workflow.Status;
        const string reason = "Customer requested order cancellation.";
        workflow.MarkCancelled(ErrorCodes.OrderCancelledByCustomer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (statusBeforeCancellation >= OrderWorkflowStatus.InventoryReserved)
        {
            await publisher.ReleaseInventoryAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                reason,
                cancellationToken);
        }

        await publisher.CancelOrderAsync(
            workflow,
            message.CorrelationId,
            message.MessageId,
            ErrorCodes.OrderCancelledByCustomer,
            reason,
            cancellationToken);
    }
}
