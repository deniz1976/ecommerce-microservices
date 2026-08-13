using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class InventoryWorkflowService(
    OrderWorkflowLoader loader,
    IUnitOfWork unitOfWork,
    IWorkflowCommandPublisher publisher)
{
    public async Task HandleReservedAsync(
        InventoryReserved message,
        DateTimeOffset now,
        TimeSpan paymentTimeout,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            await publisher.ReleaseInventoryAsync(
                workflow, message.CorrelationId, message.MessageId,
                "Customer requested order cancellation.", cancellationToken);
            return;
        }

        if (workflow.Status != OrderWorkflowStatus.Submitted)
        {
            return;
        }

        workflow.MarkInventoryReserved(now.Add(paymentTimeout));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.AuthorizePaymentAsync(
            workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleFailedAsync(
        InventoryReservationFailed message,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.CancelOrderAsync(
            workflow, message.CorrelationId, message.MessageId,
            message.ReasonCode, message.Reason, cancellationToken);
    }
}
