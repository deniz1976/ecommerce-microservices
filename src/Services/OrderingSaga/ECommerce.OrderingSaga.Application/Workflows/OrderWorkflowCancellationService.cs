using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowCancellationService(
    IRepository<OrderWorkflow, Guid> repository,
    IUnitOfWork unitOfWork,
    IOrderWorkflowIdentityReader identityReader,
    IWorkflowCommandPublisher publisher)
{
    public async Task HandleAsync(
        OrderCancellationRequested message,
        CancellationToken cancellationToken)
    {
        Guid? workflowId = await identityReader.FindIdByOrderIdAsync(
            message.OrderId,
            cancellationToken);
        OrderWorkflow? workflow = workflowId is null
            ? null
            : await repository.GetByIdAsync(workflowId.Value, cancellationToken);
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
