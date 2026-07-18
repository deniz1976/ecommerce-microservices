using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.BuildingBlocks.EventBus;

public static class OutboxRegistrationExtensions
{
    public static IBusRegistrationConfigurator AddPostgresEntityFrameworkOutbox<TDbContext>(
        this IBusRegistrationConfigurator registration)
        where TDbContext : DbContext
    {
        registration.AddEntityFrameworkOutbox<TDbContext>(options =>
        {
            options.UsePostgres();
            options.UseBusOutbox();
            options.QueryDelay = TimeSpan.FromSeconds(5);
            options.DuplicateDetectionWindow = TimeSpan.FromMinutes(10);
        });

        registration.AddConfigureEndpointsCallback((context, _, endpoint) =>
        {
            endpoint.UseMessageRetry(retry => retry.Intervals(
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1)));
            endpoint.UseEntityFrameworkOutbox<TDbContext>(context);
        });

        return registration;
    }
}
