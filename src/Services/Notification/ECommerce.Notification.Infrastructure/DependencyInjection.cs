using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<NotificationDbContext>(configuration, "NotificationDb");
        services.AddScoped<INotificationRepository, NotificationRepository>();
        return services;
    }
}
