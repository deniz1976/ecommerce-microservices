using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeWorkflowCommandPublisher : IWorkflowCommandPublisher
{
    public int ReleaseCount { get; private set; }

    public int ShipCount { get; private set; }

    public int RefundCount { get; private set; }

    public int CancelCount { get; private set; }

    public int CancelShipmentCount { get; private set; }

    public int RejectCancellationCount { get; private set; }

    public string? CancellationReasonCode { get; private set; }

    public Task ReserveInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task ReleaseInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken)
    {
        ReleaseCount++;
        return Task.CompletedTask;
    }

    public Task ShipInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        ShipCount++;
        return Task.CompletedTask;
    }

    public Task AuthorizePaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task RefundPaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken)
    {
        RefundCount++;
        return Task.CompletedTask;
    }

    public Task CreateShipmentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task CancelShipmentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken)
    {
        CancelShipmentCount++;
        return Task.CompletedTask;
    }

    public Task ConfirmOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task CancelOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reasonCode, string reason, CancellationToken cancellationToken)
    {
        CancelCount++;
        CancellationReasonCode = reasonCode;
        return Task.CompletedTask;
    }

    public Task RejectOrderCancellationAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
    {
        RejectCancellationCount++;
        return Task.CompletedTask;
    }
}
