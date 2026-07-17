using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<NotificationDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__NotificationDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("notification_db", "notification_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new NotificationDbContext(builder.Options);
    }
}
