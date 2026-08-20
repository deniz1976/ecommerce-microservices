using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;
using ECommerce.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Notification.Infrastructure.Delivery;

public sealed class NotificationDispatchProcessor(
    NotificationDbContext dbContext,
    IEnumerable<INotificationDeliveryProvider> providers,
    IOptions<NotificationDeliveryOptions> options,
    TimeProvider timeProvider,
    ILogger<NotificationDispatchProcessor> logger)
{
    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        NotificationDispatchChannel[] enabledChannels = options.Value.EnabledChannels;
        NotificationDispatch[] dispatches = await dbContext.NotificationDispatches
            .Where(dispatch =>
                dispatch.Status == NotificationDispatchStatus.Pending &&
                enabledChannels.Contains(dispatch.Channel) &&
                dispatch.NextAttemptAt <= now)
            .OrderBy(dispatch => dispatch.NextAttemptAt)
            .Take(options.Value.BatchSize)
            .ToArrayAsync(cancellationToken);
        Dictionary<NotificationDispatchChannel, INotificationDeliveryProvider> providersByChannel =
            providers.ToDictionary(provider => provider.Channel);

        foreach (NotificationDispatch dispatch in dispatches)
        {
            NotificationRecord notification = await dbContext.Notifications
                .SingleAsync(item => item.Id == dispatch.NotificationId, cancellationToken);
            NotificationDelivery delivery = new(
                dispatch.Id,
                notification.CustomerId,
                notification.OrderId,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.Culture,
                dispatch.Channel);

            NotificationDeliveryResult result = NotificationDeliveryResult.RetryableFailure;
            try
            {
                result = await providersByChannel[dispatch.Channel]
                    .DeliverAsync(delivery, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Notification dispatch {DispatchId} through {Channel} failed and will follow the retry policy.",
                    dispatch.Id,
                    dispatch.Channel);
            }

            DateTimeOffset completedAt = timeProvider.GetUtcNow();
            if (result == NotificationDeliveryResult.Delivered)
            {
                dispatch.MarkDelivered(completedAt);
            }
            else if (result == NotificationDeliveryResult.PermanentFailure)
            {
                dispatch.MarkPermanentlyFailed();
            }
            else
            {
                dispatch.MarkFailed(completedAt, options.Value.MaxAttempts);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return dispatches.Length;
    }
}
