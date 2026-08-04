using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Notification.Application.Notifications;

public sealed class NotificationHistoryService(INotificationHistoryReader reader)
{
    public async Task<PagedResult<NotificationMessage>> GetAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken)
    {
        int normalizedPageNumber = Math.Clamp(pageNumber, 1, 10_000);
        int normalizedPageSize = Math.Clamp(pageSize, 1, 50);
        NotificationHistoryPage page = await reader.SearchByCustomerAsync(
            customerId,
            normalizedPageNumber,
            normalizedPageSize,
            unreadOnly,
            cancellationToken);

        return new PagedResult<NotificationMessage>(
            page.Items.Select(NotificationMapper.ToMessage).ToArray(),
            normalizedPageNumber,
            normalizedPageSize,
            page.TotalCount);
    }
}
