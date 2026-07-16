using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.EventBus;

public static class DependencyInjection
{
    public static IServiceCollection AddECommerceMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[] consumerAssemblies,
        Action<IBusRegistrationConfigurator>? configureRegistration = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureBus = null)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddMassTransit(registration =>
        {
            registration.SetKebabCaseEndpointNameFormatter();
            registration.AddConsumers(consumerAssemblies);
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
