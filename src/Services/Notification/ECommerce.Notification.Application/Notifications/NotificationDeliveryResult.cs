namespace ECommerce.Notification.Application.Notifications;

public enum NotificationDeliveryResult
{
    Delivered = 1,
    RetryableFailure = 2,
    PermanentFailure = 3
}
