using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Ordering.Domain;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Infrastructure.Catalog;
using ECommerce.Ordering.Infrastructure.Messaging;
using ECommerce.Ordering.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<OrderingDbContext>(configuration, "OrderingDb");
        services.AddScoped<IRepository<Order, Guid>>(serviceProvider =>
            new EfRepository<Order, Guid>(
                serviceProvider.GetRequiredService<OrderingDbContext>(),
                order => order.Id));
        services.AddScoped<IUnitOfWork, OrderingUnitOfWork>();
        services.AddScoped<IOrderReader, OrderReader>();
        services.AddOptions<CatalogClientOptions>()
            .Bind(configuration.GetSection(CatalogClientOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                "CatalogClient:BaseUrl must be an absolute HTTP or HTTPS URL.")
            .Validate(
                options => options.TimeoutSeconds is >= 1 and <= 30,
                "CatalogClient:TimeoutSeconds must be between 1 and 30.")
            .ValidateOnStart();
        services.AddHttpClient<IStoreOrderAccessAuthorizer, CatalogStoreOrderAccessAuthorizer>(
            (serviceProvider, client) =>
            {
                CatalogClientOptions options = serviceProvider
                    .GetRequiredService<IOptions<CatalogClientOptions>>()
                    .Value;
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
        services.AddScoped<IOrderSubmittedPublisher, MassTransitOrderSubmittedPublisher>();
        services.AddScoped<
            IOrderCancellationRequestedPublisher,
            MassTransitOrderCancellationRequestedPublisher>();
        return services;
    }
}
