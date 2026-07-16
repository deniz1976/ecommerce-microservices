using Ocelot.DependencyInjection;

namespace ECommerce.ApiGateway.Configuration;

public static class GatewayServiceCollectionExtensions
{
    public static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOcelot(configuration);
        services.AddHealthChecks();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                string[] allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
