using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Notification.Domain;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Infrastructure.Persistence;
using ECommerce.Notification.Infrastructure.Delivery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<NotificationDbContext>(configuration, "NotificationDb");
        services.AddScoped<IRepository<NotificationRecord, Guid>>(serviceProvider =>
            new EfRepository<NotificationRecord, Guid>(
                serviceProvider.GetRequiredService<NotificationDbContext>(),
                notification => notification.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<NotificationDbContext>>();
        services.AddScoped<NotificationReader>();
        services.AddScoped<INotificationReader>(
            serviceProvider => serviceProvider.GetRequiredService<NotificationReader>());
        services.AddScoped<INotificationHistoryReader>(
            serviceProvider => serviceProvider.GetRequiredService<NotificationReader>());
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<NotificationDeliveryOptions>()
            .Bind(configuration.GetSection(NotificationDeliveryOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<NotificationDeliveryOptions>, NotificationDeliveryOptionsValidator>();
        services.AddScoped<INotificationCreationStore, NotificationCreationStore>();
        services.AddScoped<NotificationDispatchProcessor>();
        services.AddHostedService<NotificationDispatchBackgroundService>();
        return services;
    }
}
