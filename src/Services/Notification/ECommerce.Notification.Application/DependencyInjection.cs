using ECommerce.Notification.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddScoped<NotificationService>();
        return services;
    }
}
