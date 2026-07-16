using Npgsql;

namespace ECommerce.RuntimeChecks.Probes;

internal sealed class PostgresWorkflowProbe
{
    public async Task<bool> HasExpectedValueAsync<T>(
        string connectionName,
        string sql,
        Guid orderId,
        T expectedValue,
        CancellationToken cancellationToken)
    {
        T? value = await QueryScalarAsync<T>(connectionName, sql, orderId, cancellationToken);
        return EqualityComparer<T>.Default.Equals(value!, expectedValue);
    }

    public async Task<bool> HasPositiveValueAsync(
        string connectionName,
        string sql,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        long? value = await QueryScalarAsync<long>(connectionName, sql, orderId, cancellationToken);
        return value > 0;
    }

    public async Task<bool> HasMinimumValueAsync(
        string connectionName,
        string sql,
        Guid orderId,
        long minimumValue,
        CancellationToken cancellationToken)
    {
        long? value = await QueryScalarAsync<long>(connectionName, sql, orderId, cancellationToken);
        return value >= minimumValue;
    }

    private static async Task<T?> QueryScalarAsync<T>(
        string connectionName,
        string sql,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        string connectionString = Environment.GetEnvironmentVariable(connectionName)
            ?? throw new InvalidOperationException($"{connectionName} is not set.");

        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("order_id", orderId);

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null || value is DBNull)
        {
            return default;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
