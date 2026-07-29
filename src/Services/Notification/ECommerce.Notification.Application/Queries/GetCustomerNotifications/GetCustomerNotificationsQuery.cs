using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Application.Queries.GetCustomerNotifications;

public sealed record GetCustomerNotificationsQuery(
    Guid CustomerId,
    int PageNumber,
    int PageSize,
    bool UnreadOnly) : IQuery<PagedResult<NotificationMessage>>;
