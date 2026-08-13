using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Infrastructure.Catalog;
using ECommerce.Basket.Infrastructure.Messaging;
using ECommerce.Basket.Infrastructure.Persistence;
using ECommerce.Basket.Infrastructure.Redis;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Basket.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<BasketDbContext>(configuration, "BasketDb");
        services.AddScoped<IRepository<BasketCheckoutSnapshot, Guid>>(serviceProvider =>
            new EfRepository<BasketCheckoutSnapshot, Guid>(
                serviceProvider.GetRequiredService<BasketDbContext>(),
                snapshot => snapshot.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<BasketDbContext>>();
        services.AddScoped<ICheckoutPublisher, MassTransitCheckoutPublisher>();
        services.AddOptions<CatalogClientOptions>()
            .Bind(configuration.GetSection(CatalogClientOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "CatalogClient:BaseUrl must be an absolute URL.")
            .Validate(
                options => options.TimeoutSeconds > 0,
                "CatalogClient:TimeoutSeconds must be greater than zero.")
            .ValidateOnStart();
        services.AddHttpClient<IProductCatalogReader, CatalogProductReader>((serviceProvider, client) =>
        {
            CatalogClientOptions options = serviceProvider.GetRequiredService<IOptions<CatalogClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Endpoint),
                "Redis:Endpoint is required.")
            .Validate(
                options => RedisOptions.IsBasketTtlValid(options.BasketTtlHours),
                $"Redis:BasketTtlHours must be between {RedisOptions.MinimumBasketTtlHours} and {RedisOptions.MaximumBasketTtlHours} hours.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<BasketExpirationPolicy>();
        services.AddSingleton<BasketStoreMetrics>();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            RedisOptions options = serviceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.Endpoint!);
        });
        services.AddScoped<IActiveBasketStore, RedisActiveBasketStore>();

        return services;
    }
}
