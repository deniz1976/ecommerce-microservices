namespace ECommerce.BuildingBlocks.EventBus;

public sealed class EventBusMonitoringOptions
{
    public const string SectionName = "EventBusMonitoring";

    public int OutboxPollIntervalSeconds { get; init; } = 30;

    public int OutboxWarningAgeSeconds { get; init; } = 60;
}
