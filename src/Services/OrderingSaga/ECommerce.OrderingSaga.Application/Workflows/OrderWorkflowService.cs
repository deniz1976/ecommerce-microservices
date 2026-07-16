using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowService
{
    private readonly IOrderWorkflowRepository repository;
    private readonly IWorkflowCommandPublisher publisher;

    public OrderWorkflowService(IOrderWorkflowRepository repository, IWorkflowCommandPublisher publisher)
    {
        this.repository = repository;
        this.publisher = publisher;
    }

    public async Task HandleAsync(OrderSubmitted message, CancellationToken cancellationToken)
    {
        OrderWorkflow? existing = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
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
            message.PostalCode);

        foreach (ECommerce.BuildingBlocks.Contracts.Orders.OrderLine item in message.Items)
        {
            workflow.AddItem(item.ProductId, item.Quantity);
        }

        repository.Add(workflow);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.ReserveInventoryAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(InventoryReserved message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null || workflow.Status != OrderWorkflowStatus.Submitted)
        {
            return;
        }

        workflow.MarkInventoryReserved();
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.AuthorizePaymentAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(InventoryReservationFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }

    public async Task HandleAsync(PaymentAuthorized message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null || workflow.Status != OrderWorkflowStatus.InventoryReserved)
        {
            return;
        }

        workflow.MarkPaymentAuthorized();
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.CreateShipmentAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(PaymentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.ReleaseInventoryAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }

    public async Task HandleAsync(ShipmentCreated message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null || workflow.Status != OrderWorkflowStatus.PaymentAuthorized)
        {
            return;
        }

        workflow.MarkShipmentCreated();
        workflow.MarkCompleted();
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.ConfirmOrderAsync(workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }

    public async Task HandleAsync(ShipmentFailed message, CancellationToken cancellationToken)
    {
        OrderWorkflow? workflow = await repository.GetByOrderIdAsync(message.OrderId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.MarkCancelled(message.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.RefundPaymentAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.ReleaseInventoryAsync(workflow, message.CorrelationId, message.MessageId, message.Reason, cancellationToken);
        await publisher.CancelOrderAsync(workflow, message.CorrelationId, message.MessageId, message.ReasonCode, message.Reason, cancellationToken);
    }
}
