using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class ShippingWorkflowService(
    OrderWorkflowLoader loader,
    IUnitOfWork unitOfWork,
    IWorkflowCommandPublisher publisher)
{
    private const string LateShipmentReason = "The order was cancelled before the shipment was created.";

    public async Task HandleCreatedAsync(ShipmentCreated message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            await publisher.CancelShipmentAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                LateShipmentReason,
                cancellationToken);
            return;
        }

        if (workflow.Status != OrderWorkflowStatus.PaymentAuthorized)
        {
            return;
        }

        workflow.MarkShipmentCreated();
        workflow.MarkCompleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ShipInventoryAsync(
            workflow, message.CorrelationId, message.MessageId, cancellationToken);
        await publisher.ConfirmOrderAsync(
            workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleFailedAsync(ShipmentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null || workflow.Status != OrderWorkflowStatus.PaymentAuthorized)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.RefundPaymentAsync(
            workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.ReleaseInventoryAsync(
            workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(
            workflow, message.CorrelationId, message.MessageId,
            message.ReasonCode, message.Reason, cancellationToken);
    }
}
