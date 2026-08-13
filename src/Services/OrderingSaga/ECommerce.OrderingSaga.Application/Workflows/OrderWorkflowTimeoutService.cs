using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowTimeoutService
{
    private const string TimeoutReason = "A workflow step exceeded its processing deadline.";
    private readonly IRepository<OrderWorkflow, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IWorkflowCommandPublisher publisher;

    public OrderWorkflowTimeoutService(
        IRepository<OrderWorkflow, Guid> repository,
        IUnitOfWork unitOfWork,
        IWorkflowCommandPublisher publisher)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.publisher = publisher;
    }

    public async Task HandleAsync(
        Guid workflowId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByIdAsync(workflowId, cancellationToken);
        if (workflow is null || !workflow.IsTimeoutDue(now))
        {
            return;
        }

        OrderWorkflowStatus timedOutStatus = workflow.Status;
        string reasonCode = GetReasonCode(timedOutStatus);
        workflow.MarkTimedOut(reasonCode, now);

        if (timedOutStatus == OrderWorkflowStatus.PaymentAuthorized)
        {
            await publisher.RefundPaymentAsync(
                workflow,
                workflow.CorrelationId,
                null,
                TimeoutReason,
                cancellationToken);
        }

        if (timedOutStatus is OrderWorkflowStatus.InventoryReserved or
            OrderWorkflowStatus.PaymentAuthorized)
        {
            await publisher.ReleaseInventoryAsync(
                workflow,
                workflow.CorrelationId,
                null,
                TimeoutReason,
                cancellationToken);
        }

        await publisher.CancelOrderAsync(
            workflow,
            workflow.CorrelationId,
            null,
            reasonCode,
            TimeoutReason,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GetReasonCode(OrderWorkflowStatus status)
    {
        return status switch
        {
            OrderWorkflowStatus.Submitted => ErrorCodes.InventoryTimeout,
            OrderWorkflowStatus.InventoryReserved => ErrorCodes.PaymentTimeout,
            OrderWorkflowStatus.PaymentAuthorized => ErrorCodes.ShipmentTimeout,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
