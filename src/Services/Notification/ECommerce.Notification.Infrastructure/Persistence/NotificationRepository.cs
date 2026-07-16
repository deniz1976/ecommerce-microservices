using ECommerce.Notification.Application.Notifications;

namespace ECommerce.Notification.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        this.dbContext = dbContext;
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
