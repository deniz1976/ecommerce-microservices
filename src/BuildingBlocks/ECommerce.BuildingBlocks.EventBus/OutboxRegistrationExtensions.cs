using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.BuildingBlocks.EventBus;

public static class OutboxRegistrationExtensions
{
    internal const int ConcurrencyRetryLimit = 8;

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
            endpoint.UseECommerceMessageRetry();
            endpoint.UseEntityFrameworkOutbox<TDbContext>(context);
        });

        return registration;
    }

    internal static void UseECommerceMessageRetry(this IReceiveEndpointConfigurator endpoint)
    {
        endpoint.UseMessageRetry(retry =>
        {
            retry.Ignore<DbUpdateConcurrencyException>();
            retry.Intervals(
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1));
        });
        endpoint.UseMessageRetry(retry =>
        {
            retry.Handle<DbUpdateConcurrencyException>();
            retry.Exponential(
                ConcurrencyRetryLimit,
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(100));
        });
    }
}
