namespace ECommerce.Notification.Domain;

public sealed class NotificationDispatch
{
    private NotificationDispatch()
    {
    }

    public NotificationDispatch(
        Guid notificationId,
        NotificationDispatchChannel channel,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        NotificationId = notificationId;
        Channel = channel;
        Status = NotificationDispatchStatus.Pending;
        CreatedAt = createdAt.ToUniversalTime();
        NextAttemptAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid NotificationId { get; private set; }

    public NotificationDispatchChannel Channel { get; private set; }

    public NotificationDispatchStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }

    public void MarkDelivered(DateTimeOffset deliveredAt)
    {
        if (Status != NotificationDispatchStatus.Pending)
        {
            return;
        }

        AttemptCount++;
        Status = NotificationDispatchStatus.Delivered;
        DeliveredAt = deliveredAt.ToUniversalTime();
    }

    public void MarkFailed(DateTimeOffset failedAt, int maxAttempts)
    {
        if (Status != NotificationDispatchStatus.Pending)
        {
            return;
        }

        AttemptCount++;
        if (AttemptCount >= maxAttempts)
        {
            Status = NotificationDispatchStatus.Exhausted;
            return;
        }

        double delayMinutes = Math.Pow(2, Math.Min(AttemptCount - 1, 6));
        NextAttemptAt = failedAt.ToUniversalTime().AddMinutes(delayMinutes);
    }

    public void MarkPermanentlyFailed()
    {
        if (Status == NotificationDispatchStatus.Pending)
        {
            AttemptCount++;
            Status = NotificationDispatchStatus.Exhausted;
        }
    }
}
