namespace ECommerce.OrderingSaga.Application.Workflows;

public interface IOrderWorkflowTimeoutReader
{
    Task<IReadOnlyCollection<Guid>> FindDueWorkflowIdsAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken);
}
