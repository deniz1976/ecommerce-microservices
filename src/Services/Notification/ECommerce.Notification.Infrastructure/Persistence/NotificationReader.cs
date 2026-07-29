using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Notification.Infrastructure.Persistence;

public sealed class NotificationReader : INotificationReader, INotificationHistoryReader
{
    private readonly NotificationDbContext dbContext;

    public NotificationReader(NotificationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<NotificationRecord?> FindBySourceAsync(
        Guid sourceMessageId,
        NotificationChannel channel,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                notification => notification.SourceMessageId == sourceMessageId && notification.Channel == channel,
                cancellationToken);
    }

    public async Task<NotificationHistoryPage> SearchByCustomerAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationRecord> query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.CustomerId == customerId);
        if (unreadOnly)
        {
            query = query.Where(notification => notification.ReadAt == null);
        }

        long totalCount = await query.LongCountAsync(cancellationToken);
        NotificationRecord[] items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new NotificationHistoryPage(items, totalCount);
    }
}
