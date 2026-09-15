using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;
using ECommerce.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Notification.Infrastructure.Delivery;

public sealed class NotificationCreationStore(
    NotificationDbContext dbContext,
    IOptions<NotificationDeliveryOptions> options,
    TimeProvider timeProvider)
    : INotificationCreationStore
{
    public async Task<NotificationCreationResult> CreateIfMissingAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationRecord? existing = await dbContext.Notifications
            .SingleOrDefaultAsync(
                notification =>
                    notification.SourceMessageId == request.SourceMessageId &&
                    notification.Channel == NotificationChannel.Realtime,
                cancellationToken);
        if (existing is not null)
        {
            return new NotificationCreationResult(existing, false);
        }

        NotificationRecord notification = new(
            request.SourceMessageId,
            request.CustomerId,
            request.OrderId,
            request.Type,
            request.Title,
            request.Message,
            request.Culture,
            request.ReasonCode,
            request.TrackingNumber,
            NotificationChannel.Realtime);
        dbContext.Notifications.Add(notification);

        DateTimeOffset createdAt = timeProvider.GetUtcNow();
        foreach (NotificationDispatchChannel channel in options.Value.EnabledChannels.Distinct())
        {
            dbContext.NotificationDispatches.Add(
                new NotificationDispatch(notification.Id, channel, createdAt));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new NotificationCreationResult(notification, true);
    }
}
