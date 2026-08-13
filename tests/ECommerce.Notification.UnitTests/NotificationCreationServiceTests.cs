using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.UnitTests;

public sealed class NotificationCreationServiceTests
{
    [Fact]
    public async Task CreateAsyncPersistsAndPublishesNewSourceMessage()
    {
        FakeNotificationRepository repository = new();
        FakeLiveNotificationPublisher publisher = new();
        NotificationCreationService service = CreateService(repository, publisher);
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
        NotificationCreationService service = CreateService(repository, publisher);
        CreateNotificationRequest request = CreateRequest();
        NotificationMessage first = await service.CreateAsync(request, CancellationToken.None);

        NotificationMessage duplicate = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(first, duplicate);
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Single(publisher.PublishedMessages);
    }

    private static NotificationCreationService CreateService(
        FakeNotificationRepository repository,
        FakeLiveNotificationPublisher publisher)
    {
        return new NotificationCreationService(
            repository,
            repository,
            repository,
            publisher);
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
