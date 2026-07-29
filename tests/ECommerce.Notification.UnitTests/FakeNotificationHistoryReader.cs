using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.UnitTests;

internal sealed class FakeNotificationHistoryReader : INotificationHistoryReader
{
    public Guid CustomerId { get; private set; }

    public int PageNumber { get; private set; }

    public int PageSize { get; private set; }

    public bool UnreadOnly { get; private set; }

    public Task<NotificationHistoryPage> SearchByCustomerAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken)
    {
        CustomerId = customerId;
        PageNumber = pageNumber;
        PageSize = pageSize;
        UnreadOnly = unreadOnly;
        return Task.FromResult(
            new NotificationHistoryPage([], 0));
    }
}
