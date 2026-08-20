using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Infrastructure.Delivery;

public sealed class NotificationDeliveryOptions
{
    public const string SectionName = "NotificationDelivery";

    public NotificationDispatchChannel[] EnabledChannels { get; init; } = [];

    public int PollIntervalSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 25;

    public int MaxAttempts { get; init; } = 5;
}
