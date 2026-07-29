namespace ECommerce.Notification.Application.Notifications;

public interface INotificationHistoryReader
{
    Task<NotificationHistoryPage> SearchByCustomerAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken);
}
