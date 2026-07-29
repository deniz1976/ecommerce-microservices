using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Queries.GetCustomerNotifications;

public sealed class GetCustomerNotificationsQueryHandler
    : IQueryHandler<GetCustomerNotificationsQuery, PagedResult<NotificationMessage>>
{
    private readonly INotificationHistoryReader reader;

    public GetCustomerNotificationsQueryHandler(INotificationHistoryReader reader)
    {
        this.reader = reader;
    }

    public async Task<PagedResult<NotificationMessage>> HandleAsync(
        GetCustomerNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        int pageNumber = Math.Clamp(query.PageNumber, 1, 10_000);
        int pageSize = Math.Clamp(query.PageSize, 1, 50);
        NotificationHistoryPage page = await reader.SearchByCustomerAsync(
            query.CustomerId,
            pageNumber,
            pageSize,
            query.UnreadOnly,
            cancellationToken);

        return new PagedResult<NotificationMessage>(
            page.Items.Select(NotificationMapper.ToMessage).ToArray(),
            pageNumber,
            pageSize,
            page.TotalCount);
    }
}
