using ECommerce.Notification.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    public DbSet<NotificationDispatch> NotificationDispatches => Set<NotificationDispatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
