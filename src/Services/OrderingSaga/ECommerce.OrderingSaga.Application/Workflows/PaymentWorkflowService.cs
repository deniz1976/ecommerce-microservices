using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class PaymentWorkflowService(
    OrderWorkflowLoader loader,
    IUnitOfWork unitOfWork,
    IWorkflowCommandPublisher publisher)
{
    public async Task HandleAuthorizedAsync(
        PaymentAuthorized message,
        DateTimeOffset now,
        TimeSpan shippingTimeout,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            const string reason = "Customer requested order cancellation.";
            await publisher.RefundPaymentAsync(
                workflow, message.CorrelationId, message.MessageId, reason, cancellationToken);
            await publisher.ReleaseInventoryAsync(
                workflow, message.CorrelationId, message.MessageId, reason, cancellationToken);
            return;
        }

        if (workflow.Status != OrderWorkflowStatus.InventoryReserved)
        {
            return;
        }

        workflow.MarkPaymentAuthorized(now.Add(shippingTimeout));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.CreateShipmentAsync(
            workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleFailedAsync(PaymentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ReleaseInventoryAsync(
            workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(
            workflow, message.CorrelationId, message.MessageId,
            message.ReasonCode, message.Reason, cancellationToken);
    }
}
