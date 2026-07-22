using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.EventBus;

public static class DependencyInjection
{
    public static IServiceCollection AddECommerceMassTransit<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string endpointNamePrefix,
        Assembly[] consumerAssemblies,
        Action<IBusRegistrationConfigurator>? configureRegistration = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureBus = null)
        where TDbContext : DbContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointNamePrefix);
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<EventBusMonitoringOptions>(configuration.GetSection(EventBusMonitoringOptions.SectionName));
        services.AddSingleton(new OutboxMetrics(serviceName));
        services.AddHostedService<OutboxMonitoringService<TDbContext>>();

        services.AddMassTransit(registration =>
        {
            registration.SetEndpointNameFormatter(
                new KebabCaseEndpointNameFormatter(endpointNamePrefix, includeNamespace: false));
            registration.AddConsumers(consumerAssemblies);
            registration.AddPostgresEntityFrameworkOutbox<TDbContext>();
            configureRegistration?.Invoke(registration);

            registration.UsingRabbitMq((context, cfg) =>
            {
                RabbitMqOptions options = (configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions()).Normalize();

                cfg.Host(options.Host, options.Port, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);

                    if (options.UseSsl)
                    {
                        host.UseSsl(ssl => { });
                    }
                });

                configureBus?.Invoke(context, cfg);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
