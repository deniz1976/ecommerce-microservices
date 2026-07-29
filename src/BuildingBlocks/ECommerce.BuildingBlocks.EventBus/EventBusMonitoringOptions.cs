namespace ECommerce.BuildingBlocks.EventBus;

public sealed class EventBusMonitoringOptions
{
    public const string SectionName = "EventBusMonitoring";

    public int OutboxPollIntervalSeconds { get; init; } = 30;

    public int OutboxWarningAgeSeconds { get; init; } = 60;

    internal TimeSpan PollInterval =>
        TimeSpan.FromSeconds(Math.Clamp(OutboxPollIntervalSeconds, 5, 300));

    internal TimeSpan WarningAge =>
        TimeSpan.FromSeconds(Math.Clamp(OutboxWarningAgeSeconds, 30, 3600));
}
