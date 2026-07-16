namespace ECommerce.BuildingBlocks.Persistence;

public sealed class PostgresOptions
{
    public int MaxRetryCount { get; init; } = 5;

    public int MaxRetryDelaySeconds { get; init; } = 10;

    public int CommandTimeoutSeconds { get; init; } = 30;
}
