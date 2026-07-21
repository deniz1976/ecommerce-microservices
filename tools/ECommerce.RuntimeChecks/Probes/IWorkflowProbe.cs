namespace ECommerce.RuntimeChecks.Probes;

internal interface IWorkflowProbe
{
    Task<bool> HasExpectedValueAsync<T>(
        string connectionName,
        string sql,
        Guid orderId,
        T expectedValue,
        CancellationToken cancellationToken);

    Task<bool> HasPositiveValueAsync(
        string connectionName,
        string sql,
        Guid orderId,
        CancellationToken cancellationToken);

    Task<bool> HasMinimumValueAsync(
        string connectionName,
        string sql,
        Guid orderId,
        long minimum,
        CancellationToken cancellationToken);
}
