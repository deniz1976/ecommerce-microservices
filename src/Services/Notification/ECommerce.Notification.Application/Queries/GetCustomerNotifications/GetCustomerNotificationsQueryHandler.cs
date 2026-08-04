using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Queries.GetCustomerNotifications;

public sealed class GetCustomerNotificationsQueryHandler
    : IQueryHandler<GetCustomerNotificationsQuery, PagedResult<NotificationMessage>>
{
    private readonly NotificationHistoryService historyService;

    public GetCustomerNotificationsQueryHandler(NotificationHistoryService historyService)
    {
        this.historyService = historyService;
    }

    public async Task<PagedResult<NotificationMessage>> HandleAsync(
        GetCustomerNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        return await historyService.GetAsync(
            query.CustomerId,
            query.PageNumber,
            query.PageSize,
            query.UnreadOnly,
            cancellationToken);
    }
}
