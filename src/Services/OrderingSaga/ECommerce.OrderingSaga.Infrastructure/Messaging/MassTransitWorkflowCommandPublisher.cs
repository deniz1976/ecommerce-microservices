using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Inventory;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;
using MassTransit;

namespace ECommerce.OrderingSaga.Infrastructure.Messaging;

public sealed class MassTransitWorkflowCommandPublisher : IWorkflowCommandPublisher
{
    private readonly IPublishEndpoint publishEndpoint;

    public MassTransitWorkflowCommandPublisher(IPublishEndpoint publishEndpoint)
    {
        this.publishEndpoint = publishEndpoint;
    }

    public Task ReserveInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new ReserveInventory(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.Items.Select(x => new InventoryReservationLine(x.ProductId, x.Quantity)).ToArray()), cancellationToken);
    }

    public Task ReleaseInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new ReleaseInventory(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.Items.Select(x => new InventoryReleaseLine(x.ProductId, x.Quantity)).ToArray(),
            reason), cancellationToken);
    }

    public Task ShipInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new ShipInventory(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.Items.Select(x => new InventoryReleaseLine(x.ProductId, x.Quantity)).ToArray()),
            cancellationToken);
    }

    public Task AuthorizePaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new AuthorizePayment(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.TotalAmount,
            workflow.Currency), cancellationToken);
    }

    public Task RefundPaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new RefundPayment(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.TotalAmount,
            workflow.Currency,
            reason), cancellationToken);
    }

    public Task CreateShipmentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new CreateShipment(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            workflow.RecipientName,
            workflow.AddressLine,
            workflow.City,
            workflow.CountryCode,
            workflow.PostalCode), cancellationToken);
    }

    public Task ConfirmOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new OrderConfirmed(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId), cancellationToken);
    }

    public Task CancelOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reasonCode, string reason, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new OrderCancelled(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId,
            reasonCode,
            reason), cancellationToken);
    }

    public Task RejectOrderCancellationAsync(
        OrderWorkflow workflow,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new OrderCancellationRejected(
            Guid.NewGuid(),
            correlationId,
            causationId,
            DateTimeOffset.UtcNow,
            ECommerce.BuildingBlocks.Contracts.Messaging.MessageDefaults.CurrentVersion,
            workflow.OrderId,
            workflow.CustomerId), cancellationToken);
    }
}
