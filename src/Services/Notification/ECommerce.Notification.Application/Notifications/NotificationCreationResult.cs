using ECommerce.Notification.Domain;

namespace ECommerce.Notification.Application.Notifications;

public sealed record NotificationCreationResult(
    NotificationRecord Notification,
    bool WasCreated);
