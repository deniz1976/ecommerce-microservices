using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Application.Queries.GetCustomerNotifications;

namespace ECommerce.Notification.UnitTests;

public sealed class GetCustomerNotificationsQueryHandlerTests
{
    [Fact]
    public async Task Query_clamps_paging_and_preserves_ownership_filter()
    {
        FakeNotificationHistoryReader reader = new();
        GetCustomerNotificationsQueryHandler handler = new(reader);
        Guid customerId = Guid.NewGuid();

        PagedResult<NotificationMessage> result = await handler.HandleAsync(
            new GetCustomerNotificationsQuery(
                customerId,
                0,
                500,
                UnreadOnly: true),
            CancellationToken.None);

        Assert.Equal(customerId, reader.CustomerId);
        Assert.Equal(1, reader.PageNumber);
        Assert.Equal(50, reader.PageSize);
        Assert.True(reader.UnreadOnly);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(50, result.PageSize);
    }
}
