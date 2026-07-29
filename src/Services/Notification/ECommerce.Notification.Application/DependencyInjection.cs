using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Commands.MarkNotificationRead;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Application.Queries.GetCustomerNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddScoped<NotificationService>();
        services.AddScoped<
            ICommandHandler<CreateNotificationCommand, NotificationMessage>,
            CreateNotificationCommandHandler>();
        services.AddScoped<
            ICommandHandler<MarkNotificationReadCommand, Result<NotificationMessage>>,
            MarkNotificationReadCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetCustomerNotificationsQuery, PagedResult<NotificationMessage>>,
            GetCustomerNotificationsQueryHandler>();
        return services;
    }
}
