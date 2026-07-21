using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Notification.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
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

    public void Add(Domain.NotificationRecord notification)
    {
        dbContext.Notifications.Add(notification);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
