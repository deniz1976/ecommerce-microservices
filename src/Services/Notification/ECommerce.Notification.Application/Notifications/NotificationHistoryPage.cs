using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed record NotificationHistoryPage(
    IReadOnlyCollection<NotificationRecord> Items,
    long TotalCount);
