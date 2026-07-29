namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationRetentionOptions
{
    public const string SectionName = "RoleReconciliationRetention";

    public bool Enabled { get; init; } = true;

    public int RetentionDays { get; init; } = 30;

    public int BatchSize { get; init; } = 100;
}
