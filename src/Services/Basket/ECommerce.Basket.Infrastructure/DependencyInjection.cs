using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Infrastructure.Persistence;
using ECommerce.Basket.Infrastructure.Redis;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<BasketDbContext>(configuration, "BasketDb");
        services.AddScoped<IBasketHistoryRepository, BasketHistoryRepository>();
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Redis:ConnectionString is required.")
            .Validate(
                options => options.BasketTtlHours > 0,
                "Redis:BasketTtlHours must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            RedisOptions options = serviceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString!);
        });
        services.AddScoped<IActiveBasketStore, RedisActiveBasketStore>();

        return services;
    }
}
