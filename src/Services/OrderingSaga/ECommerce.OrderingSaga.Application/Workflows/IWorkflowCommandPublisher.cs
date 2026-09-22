using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public interface IWorkflowCommandPublisher
{
    Task ReserveInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);

    Task ReleaseInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken);

    Task ShipInventoryAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);

    Task AuthorizePaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);

    Task RefundPaymentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reason, CancellationToken cancellationToken);

    Task CreateShipmentAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);

    Task ConfirmOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, CancellationToken cancellationToken);

    Task CancelOrderAsync(OrderWorkflow workflow, Guid correlationId, Guid? causationId, string reasonCode, string reason, CancellationToken cancellationToken);

    Task RejectOrderCancellationAsync(
        OrderWorkflow workflow,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken);
}
