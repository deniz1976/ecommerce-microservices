using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.UnitTests;

public sealed class NotificationReadStateServiceTests
{
    [Fact]
    public async Task MarkReadAsyncSetsUtcTimestampOnce()
    {
        FakeNotificationRepository repository = CreateRepositoryWithNotification();
        NotificationReadStateService service = CreateService(repository);

        Result<NotificationMessage> first = await service.MarkReadAsync(
            repository.Notification!.CustomerId,
            repository.Notification.Id,
            CancellationToken.None);
        Result<NotificationMessage> duplicate = await service.MarkReadAsync(
            repository.Notification.CustomerId,
            repository.Notification.Id,
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(TimeSpan.Zero, first.Value!.ReadAt!.Value.Offset);
        Assert.Equal(first.Value.ReadAt, duplicate.Value!.ReadAt);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task MarkReadAsyncHidesNotificationOwnedByAnotherCustomer()
    {
        FakeNotificationRepository repository = CreateRepositoryWithNotification();
        NotificationReadStateService service = CreateService(repository);

        Result<NotificationMessage> result = await service.MarkReadAsync(
            Guid.NewGuid(),
            repository.Notification!.Id,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotificationNotFound, result.Error?.Code);
        Assert.Null(repository.Notification.ReadAt);
        Assert.Equal(0, repository.SaveChangesCount);
    }

    [Fact]
    public async Task MarkAllReadAsyncMarksOwnedUnreadNotificationsOnce()
    {
        FakeNotificationRepository repository = CreateRepositoryWithNotification();
        NotificationReadStateService service = CreateService(repository);

        int first = await service.MarkAllReadAsync(
            repository.Notification!.CustomerId,
            CancellationToken.None);
        int duplicate = await service.MarkAllReadAsync(
            repository.Notification.CustomerId,
            CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(0, duplicate);
        Assert.Equal(TimeSpan.Zero, repository.Notification.ReadAt!.Value.Offset);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    private static NotificationReadStateService CreateService(
        FakeNotificationRepository repository)
    {
        return new NotificationReadStateService(
            repository,
            repository,
            repository,
            TimeProvider.System);
    }

    private static FakeNotificationRepository CreateRepositoryWithNotification()
    {
        FakeNotificationRepository repository = new();
        repository.Add(new NotificationRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.submitted",
            "Order submitted",
            "Your order has been submitted.",
            "en",
            NotificationChannel.Realtime));
        return repository;
    }
}
