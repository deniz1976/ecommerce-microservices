namespace ECommerce.RuntimeChecks.Clients;

internal interface IWorkflowEventPublisher : IAsyncDisposable
{
    Task PublishOrderCancellationRequestedAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken);
}
