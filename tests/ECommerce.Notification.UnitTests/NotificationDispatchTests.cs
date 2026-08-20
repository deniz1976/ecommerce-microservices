using ECommerce.Notification.Domain;

namespace ECommerce.Notification.UnitTests;

public sealed class NotificationDispatchTests
{
    [Fact]
    public void MarkFailedSchedulesBoundedExponentialRetryWithoutStoringFailureText()
    {
        DateTimeOffset now = new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);
        NotificationDispatch dispatch = new(Guid.NewGuid(), NotificationDispatchChannel.Email, now);

        dispatch.MarkFailed(now, 3);

        Assert.Equal(NotificationDispatchStatus.Pending, dispatch.Status);
        Assert.Equal(1, dispatch.AttemptCount);
        Assert.Equal(now.AddMinutes(1), dispatch.NextAttemptAt);
    }

    [Fact]
    public void MarkFailedExhaustsAtConfiguredAttemptLimit()
    {
        DateTimeOffset now = new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);
        NotificationDispatch dispatch = new(Guid.NewGuid(), NotificationDispatchChannel.Sms, now);

        dispatch.MarkFailed(now, 2);
        dispatch.MarkFailed(now.AddMinutes(1), 2);

        Assert.Equal(NotificationDispatchStatus.Exhausted, dispatch.Status);
        Assert.Equal(2, dispatch.AttemptCount);
        Assert.Null(dispatch.DeliveredAt);
    }

    [Fact]
    public void MarkDeliveredIsIdempotentAndUsesUtc()
    {
        DateTimeOffset createdAt = new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);
        NotificationDispatch dispatch = new(Guid.NewGuid(), NotificationDispatchChannel.Push, createdAt);
        DateTimeOffset deliveredAt = new(2026, 8, 20, 13, 0, 0, TimeSpan.FromHours(3));

        dispatch.MarkDelivered(deliveredAt);
        dispatch.MarkDelivered(deliveredAt.AddMinutes(1));

        Assert.Equal(NotificationDispatchStatus.Delivered, dispatch.Status);
        Assert.Equal(1, dispatch.AttemptCount);
        Assert.Equal(createdAt, dispatch.DeliveredAt);
    }

    [Fact]
    public void PermanentFailureExhaustsWithoutProviderDetails()
    {
        NotificationDispatch dispatch = new(
            Guid.NewGuid(),
            NotificationDispatchChannel.Email,
            DateTimeOffset.UtcNow);

        dispatch.MarkPermanentlyFailed();

        Assert.Equal(NotificationDispatchStatus.Exhausted, dispatch.Status);
        Assert.Equal(1, dispatch.AttemptCount);
    }
}
