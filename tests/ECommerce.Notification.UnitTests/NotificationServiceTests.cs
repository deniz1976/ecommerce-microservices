using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.UnitTests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task CreateAsyncPersistsAndPublishesNewSourceMessage()
    {
        FakeNotificationRepository repository = new();
        FakeLiveNotificationPublisher publisher = new();
        NotificationService service = CreateService(repository, publisher);
        CreateNotificationRequest request = CreateRequest();

        NotificationMessage result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(request.SourceMessageId, repository.Notification!.SourceMessageId);
        Assert.Equal(repository.Notification.Id, result.Id);
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Single(publisher.PublishedMessages);
    }

    [Fact]
    public async Task CreateAsyncDoesNotPersistOrPublishDuplicateSourceMessage()
    {
        FakeNotificationRepository repository = new();
        FakeLiveNotificationPublisher publisher = new();
        NotificationService service = CreateService(repository, publisher);
        CreateNotificationRequest request = CreateRequest();
        NotificationMessage first = await service.CreateAsync(request, CancellationToken.None);

        NotificationMessage duplicate = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(first, duplicate);
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Single(publisher.PublishedMessages);
    }

    [Fact]
    public async Task MarkReadAsync_sets_utc_timestamp_once()
    {
        FakeNotificationRepository repository = new();
        FakeLiveNotificationPublisher publisher = new();
        NotificationService service = CreateService(repository, publisher);
        NotificationMessage created = await service.CreateAsync(
            CreateRequest(),
            CancellationToken.None);

        Result<NotificationMessage> first = await service.MarkReadAsync(
            created.CustomerId,
            created.Id,
            CancellationToken.None);
        Result<NotificationMessage> duplicate = await service.MarkReadAsync(
            created.CustomerId,
            created.Id,
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(TimeSpan.Zero, first.Value!.ReadAt!.Value.Offset);
        Assert.Equal(first.Value.ReadAt, duplicate.Value!.ReadAt);
        Assert.Equal(2, repository.SaveChangesCount);
    }

    [Fact]
    public async Task MarkReadAsync_hides_notification_owned_by_another_customer()
    {
        FakeNotificationRepository repository = new();
        FakeLiveNotificationPublisher publisher = new();
        NotificationService service = CreateService(repository, publisher);
        NotificationMessage created = await service.CreateAsync(
            CreateRequest(),
            CancellationToken.None);

        Result<NotificationMessage> result = await service.MarkReadAsync(
            Guid.NewGuid(),
            created.Id,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotificationNotFound, result.Error?.Code);
        Assert.Null(repository.Notification!.ReadAt);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    private static NotificationService CreateService(
        FakeNotificationRepository repository,
        FakeLiveNotificationPublisher publisher)
    {
        return new NotificationService(
            repository,
            repository,
            repository,
            publisher,
            TimeProvider.System);
    }

    private static CreateNotificationRequest CreateRequest()
    {
        return new CreateNotificationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.submitted",
            "Order submitted",
            "Your order has been submitted.",
            "en");
    }
}
