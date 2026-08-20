using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;

namespace ECommerce.Notification.UnitTests;

internal sealed class FakeNotificationRepository :
    IRepository<NotificationRecord, Guid>,
    IUnitOfWork,
    INotificationReader,
    INotificationCreationStore
{
    public NotificationRecord? Notification { get; private set; }

    public int SaveChangesCount { get; private set; }

    public Task<NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        NotificationChannel channel,
        CancellationToken cancellationToken)
    {
        NotificationRecord? notification = Notification is not null &&
            Notification.SourceMessageId == sourceMessageId &&
            Notification.Channel == channel
                ? Notification
                : null;

        return Task.FromResult(notification);
    }

    public Task<IReadOnlyCollection<NotificationRecord>> GetUnreadByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<NotificationRecord> notifications =
            Notification is not null &&
            Notification.CustomerId == customerId &&
            Notification.ReadAt is null
                ? [Notification]
                : [];
        return Task.FromResult(notifications);
    }

    public void Add(NotificationRecord notification)
    {
        Notification = notification;
    }

    public Task<NotificationRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Notification?.Id == id ? Notification : null);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public async Task<NotificationCreationResult> CreateIfMissingAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationRecord? existing = await FindBySourceAsync(
            request.SourceMessageId,
            NotificationChannel.Realtime,
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
            NotificationChannel.Realtime);
        Add(notification);
        await SaveChangesAsync(cancellationToken);
        return new NotificationCreationResult(notification, true);
    }
}
