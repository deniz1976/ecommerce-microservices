using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowService
{
    private readonly IRepository<OrderWorkflow, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IOrderWorkflowIdentityReader identityReader;
    private readonly IWorkflowCommandPublisher publisher;

    public OrderWorkflowService(
        IRepository<OrderWorkflow, Guid> repository,
        IUnitOfWork unitOfWork,
        IOrderWorkflowIdentityReader identityReader,
        IWorkflowCommandPublisher publisher)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.identityReader = identityReader;
        this.publisher = publisher;
    }

    public async Task HandleAsync(
        OrderSubmitted message,
        DateTimeOffset now,
        TimeSpan inventoryTimeout,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? existing = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        OrderWorkflow workflow = new(
            message.OrderId,
            message.CustomerId,
            message.TotalAmount,
            message.Currency,
            message.RecipientName,
            message.AddressLine,
            message.City,
            message.CountryCode,
            message.PostalCode,
            message.CorrelationId,
            now.Add(inventoryTimeout));

        foreach (ECommerce.BuildingBlocks.Contracts.Orders.OrderLine item in message.Items)
        {
            workflow.AddItem(item.ProductId, item.Quantity);
        }

        repository.Add(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ReserveInventoryAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(
        InventoryReserved message,
        DateTimeOffset now,
        TimeSpan paymentTimeout,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            const string reason = "Customer requested order cancellation.";
            await publisher.ReleaseInventoryAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                reason,
                cancellationToken);
            return;
        }

        if (workflow.Status != OrderWorkflowStatus.Submitted)
        {
            return;
        }

        workflow.MarkInventoryReserved(now.Add(paymentTimeout));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.AuthorizePaymentAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(InventoryReservationFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }

    public async Task HandleAsync(
        PaymentAuthorized message,
        DateTimeOffset now,
        TimeSpan shippingTimeout,
        CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        if (workflow.Status == OrderWorkflowStatus.Cancelled)
        {
            const string reason = "Customer requested order cancellation.";
            await publisher.RefundPaymentAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                reason,
                cancellationToken);
            await publisher.ReleaseInventoryAsync(
                workflow,
                message.CorrelationId,
                message.MessageId,
                reason,
                cancellationToken);
            return;
        }

        if (workflow.Status != OrderWorkflowStatus.InventoryReserved)
        {
            return;
        }

        workflow.MarkPaymentAuthorized(now.Add(shippingTimeout));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.CreateShipmentAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(PaymentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ReleaseInventoryAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }

    public async Task HandleAsync(ShipmentCreated message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null || workflow.Status != OrderWorkflowStatus.PaymentAuthorized)
        {
            return;
        }

        workflow.MarkShipmentCreated();
        workflow.MarkCompleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ConfirmOrderAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(ShipmentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await FindByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.RefundPaymentAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.ReleaseInventoryAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }

    private async Task<OrderWorkflow?> FindByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        Guid? workflowId = await identityReader.FindIdByOrderIdAsync(orderId, cancellationToken);
        return workflowId is null
            ? null
            : await repository.GetByIdAsync(workflowId.Value, cancellationToken);
    }
}
